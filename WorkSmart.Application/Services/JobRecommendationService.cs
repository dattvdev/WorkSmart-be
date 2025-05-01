using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System.Text;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Helpers;
using WorkSmart.Core.Interface;
using WorkSmart.Repository;

namespace WorkSmart.Application.Services
{
    public class JobRecommendationService
    {
        private readonly IJobRepository _jobRepo;
        private readonly ICVRepository _cvRepo;
        private readonly IJobEmbeddingRepository _jobEmbedRepo;
        private readonly ICVEmbeddingRepository _cvEmbedRepo;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;
        private readonly string _cohereKey;
        private readonly FieldClassifierService _fieldClassifier;
        public JobRecommendationService(
            IJobRepository jobRepo,
            ICVRepository cvRepo,
            IJobEmbeddingRepository jobEmbedRepo,
            ICVEmbeddingRepository cvEmbedRepo,
            IMemoryCache cache,
            IConfiguration config,
            IMapper mapper,
             FieldClassifierService fieldClassifier)
        {
            _jobRepo = jobRepo;
            _cvRepo = cvRepo;
            _jobEmbedRepo = jobEmbedRepo;
            _cvEmbedRepo = cvEmbedRepo;
            _cache = cache;
            _cohereKey = config["Cohere:Key"]!;
            _mapper = mapper;
            _fieldClassifier = fieldClassifier;
        }

        public async Task<List<JobRecommendationDto>> GetRecommendedJobsForCV(int cvId)
        {
            string cacheKey = $"CV_RECOMMEND_{cvId}";

            if (_cache.TryGetValue(cacheKey, out List<JobRecommendationDto> cached))
                return cached;

            var cv = await _cvRepo.GetCVWithDetails(cvId);
            var activeJobs = await _jobRepo.GetAllJobActive();
            var existingEmbeddings = await _jobEmbedRepo.GetAll();
            var existingJobIds = existingEmbeddings.Select(e => e.JobID).ToHashSet();

            var missingJobs = activeJobs.Where(j => !existingJobIds.Contains(j.JobID)).ToList();

            // Chỉ embedding cho những job chưa có vector
            if (missingJobs.Any())
            {
                var texts = missingJobs.Select(BuildJobText).ToList();
                var vectors = await GetCohereEmbeddings(texts);

                for (int i = 0; i < missingJobs.Count(); i++)
                {
                    var jobId = missingJobs[i].JobID;
                    var vector = vectors[i];
                    await _jobEmbedRepo.SaveOrUpdate(jobId, JsonConvert.SerializeObject(vector));
                }
            }

            var cvEmbedding = await GetCVEmbedding(cv);
            existingEmbeddings = await _jobEmbedRepo.GetAll();
            var results = new List<(Job job, float score)>();

            foreach (var job in activeJobs)
            {
                var embedding = existingEmbeddings.FirstOrDefault(e => e.JobID == job.JobID);
                if (embedding == null) continue;

                var jobVector = JsonConvert.DeserializeObject<List<float>>(embedding.VectorJson);
                float score = CosineSimilarity(cvEmbedding, jobVector);
                if (score > 0.48f)
                    results.Add((job, score));
            }

            results = results.OrderByDescending(x => x.score).ToList();

            var finalResults = results
                .Select(r => new JobRecommendationDto { Job = _mapper.Map<JobDetailDto>(r.job), Score = r.score })
                .ToList();

            _cache.Set(cacheKey, finalResults, TimeSpan.FromMinutes(10));

            return finalResults;
        }

        public async Task UpdateJobEmbedding(Job job)
        {
            if (job.Status != JobStatus.Active) return;
            var text = BuildJobText(job);
            var vector = await GetCohereEmbedding(text);
            await _jobEmbedRepo.SaveOrUpdate(job.JobID, JsonConvert.SerializeObject(vector));
        }

        public async Task CreateEmbeddingForJobIfApproved(Job job)
        {
            if (job.Status == JobStatus.Active)
            {
                await UpdateJobEmbedding(job);
            }
        }

        private async Task<List<float>> GetCVEmbedding(CV cv)
        {
            var existing = await _cvEmbedRepo.GetByCVId(cv.CVID);
            if (existing != null && (TimeHelper.GetVietnamTime() - existing.UpdatedAt).TotalHours < 24)
                return JsonConvert.DeserializeObject<List<float>>(existing.VectorJson);

            var text = await BuildCVTextAsync(cv);
            var vector = await GetCohereEmbedding(text);

            await _cvEmbedRepo.SaveOrUpdate(cv.CVID, JsonConvert.SerializeObject(vector));
            return vector;
        }

        private async Task<string> BuildCVTextAsync(CV cv)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Full Name: {cv.FirstName} {cv.LastName}");
            sb.AppendLine($"Primary Job Position: {cv.JobPosition}");
            sb.AppendLine($"Work Type: {cv.WorkType}");

            if (cv.Summary != null)
                sb.AppendLine($"Summary: {cv.Summary}");

            if (cv.Educations != null && cv.Educations.Any())
            {
                sb.AppendLine("Education:");
                foreach (var edu in cv.Educations)
                {
                    sb.AppendLine($"- {edu.Degree} in {edu.Major} at {edu.SchoolName}");
                }
            }

            if (cv.Experiences != null && cv.Experiences.Any())
            {
                sb.AppendLine("Professional Experiences:");
                foreach (var exp in cv.Experiences)
                {
                    sb.AppendLine($"- {exp.JobPosition} at {exp.CompanyName}, focusing on {exp.Description}");
                }
            }

            if (cv.Skills != null && cv.Skills.Any())
            {
                sb.AppendLine($"Skills: {string.Join(", ", cv.Skills.Select(s => s.SkillName))}");
            }

            if (cv.Certifications != null && cv.Certifications.Any())
            {
                sb.AppendLine("Certifications:");
                foreach (var cert in cv.Certifications)
                {
                    sb.AppendLine($"- {cert.CertificateName}");
                }
            }

            var rawText = sb.ToString();

            // ✨ Gọi AI classification để enrich Field
            var field = await _fieldClassifier.ClassifyFieldFromCV(rawText);

            sb.AppendLine($"Field: {field}");

            return sb.ToString();
        }



        private string BuildJobText(Job job)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Job Title: {job.Title}");
            sb.AppendLine($"Location: {job.Location}");
            sb.AppendLine($"Work Type: {job.WorkType}");
            sb.AppendLine($"Required Education: {job.Education}");
            sb.AppendLine($"Required Experience (years): {job.Exp}");

            if (!string.IsNullOrEmpty(job.Description))
            {
                var plainDescription = System.Text.RegularExpressions.Regex.Replace(job.Description, "<.*?>", String.Empty);
                sb.AppendLine($"Job Description: {plainDescription}");
            }

            if (!string.IsNullOrEmpty(job.JobPosition))
            {
                sb.AppendLine($"Position Category: {job.JobPosition}");
            }

            if (!string.IsNullOrEmpty(job.Level))
            {
                sb.AppendLine($"Level: {job.Level}");
            }

            return sb.ToString();
        }



        private async Task<List<float>> GetCohereEmbedding(string input)
        {
            var embeddings = await GetCohereEmbeddings(new List<string> { input });
            return embeddings.First();
        }

        private async Task<List<List<float>>> GetCohereEmbeddings(List<string> inputs)
        {
            var client = new RestClient("https://api.cohere.ai");
            var request = new RestRequest("/v1/embed", Method.Post);

            request.AddHeader("Authorization", $"Bearer {_cohereKey}");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                texts = inputs,
                model = "embed-english-v3.0",
                input_type = "search_document"
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
                throw new Exception($"Cohere Embedding API error: {response.Content}");

            dynamic result = JsonConvert.DeserializeObject(response.Content);
            return ((IEnumerable<dynamic>)result.embeddings)
                .Select(vec => ((IEnumerable<dynamic>)vec).Select(x => (float)x).ToList())
                .ToList();
        }

        private float CosineSimilarity(List<float> a, List<float> b)
        {
            float dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Count; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }
            return dot / ((float)(Math.Sqrt(magA) * Math.Sqrt(magB)) + 1e-8f);
        }

        public void ClearCVRecommendationCache(int cvId)
        {
            string cacheKey = $"CV_RECOMMEND_{cvId}";
            _cache.Remove(cacheKey);
        }

        public void  DeleteCVEmbedding(int cvId) =>  _cvEmbedRepo.Delete(cvId);
        public async Task DeleteJobEmbedding(int jobId) => await _jobEmbedRepo.Delete(jobId);
        public async Task<Job?> GetJobById(int jobId) => await _jobRepo.GetById(jobId);
    }
}
