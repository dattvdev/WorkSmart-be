using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System.Text;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
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

        public JobRecommendationService(
            IJobRepository jobRepo,
            ICVRepository cvRepo,
            IJobEmbeddingRepository jobEmbedRepo,
            ICVEmbeddingRepository cvEmbedRepo,
            IMemoryCache cache,
            IConfiguration config,
            IMapper mapper)
        {
            _jobRepo = jobRepo;
            _cvRepo = cvRepo;
            _jobEmbedRepo = jobEmbedRepo;
            _cvEmbedRepo = cvEmbedRepo;
            _cache = cache;
            _cohereKey = config["Cohere:Key"]!;
            _mapper = mapper;
        }

        public async Task<List<JobRecommendationDto>> GetRecommendedJobsForCV(int cvId)
        {
            string cacheKey = $"CV_RECOMMEND_{cvId}";

            if (_cache.TryGetValue(cacheKey, out List<JobRecommendationDto> cached))
                return cached;

            var cv = await _cvRepo.GetCVWithDetails(cvId);
            var activeJobs = await _jobRepo.GetAllJobActive();

            var cvEmbedding = await GetCVEmbedding(cv);
            var jobTexts = activeJobs.Select(BuildJobText).ToList();
            var jobVectors = await GetCohereEmbeddings(jobTexts);

            var results = new List<(Job job, float score)>();

            for (int i = 0; i < activeJobs.Count; i++)
            {
                float score = CosineSimilarity(cvEmbedding, jobVectors[i]);
                if (score > 0.5f)
                    results.Add((activeJobs[i], score));
            }

            results = results.OrderByDescending(x => x.score).ToList();

            var finalResults = results
                .Select(r => new JobRecommendationDto { Job = _mapper.Map<JobDto>(r.job), Score = r.score })
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
            if (existing != null && (DateTime.Now - existing.UpdatedAt).TotalHours < 24)
                return JsonConvert.DeserializeObject<List<float>>(existing.VectorJson);

            var text = BuildCVText(cv);
            var vector = await GetCohereEmbedding(text);

            await _cvEmbedRepo.SaveOrUpdate(cv.CVID, JsonConvert.SerializeObject(vector));
            return vector;
        }

        private string BuildCVText(CV cv)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Job Position: {cv.JobPosition}");
            sb.AppendLine($"Work Type: {cv.WorkType}");
            sb.AppendLine($"Education: {string.Join(", ", cv.Educations?.Select(e => e.Degree))}");
            sb.AppendLine($"Experience: {string.Join(", ", cv.Experiences?.Select(e => e.JobPosition))}");
            sb.AppendLine($"Skills: {string.Join(", ", cv.Skills?.Select(s => s.SkillName))}");
            sb.AppendLine($"Summary: {cv.Summary}");
            return sb.ToString();
        }

        private string BuildJobText(Job job)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Title: {job.Title}");
            sb.AppendLine($"Work Type: {job.WorkType}");
            sb.AppendLine($"Location: {job.Location}");
            sb.AppendLine($"Education: {job.Education}");
            sb.AppendLine($"Experience Required: {job.Exp}");
            sb.AppendLine($"Description: {job.Description}");
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

        public async Task DeleteCVEmbedding(int cvId) => await _cvEmbedRepo.Delete(cvId);
        public async Task DeleteJobEmbedding(int jobId) => await _jobEmbedRepo.Delete(jobId);
        public async Task<Job?> GetJobById(int jobId) => await _jobRepo.GetById(jobId);
    }
}
