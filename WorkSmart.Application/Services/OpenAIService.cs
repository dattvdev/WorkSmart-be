using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto;

namespace WorkSmart.Application.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(ILogger<OpenAIService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _apiKey = configuration["OpenAI:ApiKey"];
            _baseUrl = configuration["OpenAI:BaseUrl"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<ParsedCvData> ParseCvContentAsync(string cvContent)
        {
            try
            {
                string systemPrompt = @"
You are an AI specialized in CV analysis. Please analyze the CV content into structured sections and return the result in JSON format as follows:
{
  ""firstName"": """",
  ""lastName"": """",
  ""jobPosition"": """",
  ""workType"": """",
  ""summary"": """",
  ""address"": """",
  ""phone"": """",
  ""email"": """",
  ""link"": """",
  ""educations"": [
    {
      ""major"": """",
      ""schoolName"": """",
      ""degree"": """",
      ""description"": """",
      ""startedAt"": """", 
      ""endedAt"": """"
    }
  ],
  ""experiences"": [
    {
      ""jobPosition"": """",
      ""companyName"": """",
      ""address"": """",
      ""description"": """",
      ""startedAt"": """",
      ""endedAt"": """"
    }
  ],
  ""certifications"": [
    {
      ""certificateName"": """",
      ""description"": """",
      ""createAt"": """"
    }
  ],
  ""skills"": [
    {
      ""skillName"": """",
      ""description"": """"
    }
  ]
}

Notes:
1. Split the full name into firstName and lastName
2. Time fields (startedAt, endedAt, createAt) should be returned in month/year or day/month/year format if possible
3. Extract workType (such as Full-time, Part-time, Remote, etc.) if available
4. Link can be personal website, LinkedIn, GitHub, etc.
5. Ensure valid JSON is returned
6. For each skill, add a brief description if possible
7. For the summary field, if it's not explicitly present in the CV, please generate a comprehensive professional summary based on the candidate's experience, skills, and education. This summary should highlight their key strengths, career focus, and professional value proposition in 3-5 sentences.
";

                var requestBody = new
                {
                    model = "gpt-4", // Or other suitable OpenAI model
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = systemPrompt
                        },
                        new
                        {
                            role = "user",
                            content = cvContent
                        }
                    },
                    temperature = 0.2,
                    max_tokens = 4000
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                string parsedCvJson = responseObject.choices[0].message.content.ToString();

                // Clean JSON string if needed
                if (parsedCvJson.StartsWith("```json"))
                {
                    parsedCvJson = parsedCvJson.Replace("```json", "").Replace("```", "").Trim();
                }

                var parsedData = JsonConvert.DeserializeObject<ParsedCvData>(parsedCvJson);

                // If summary is still empty after initial parsing, generate it separately
                if (string.IsNullOrEmpty(parsedData.Summary))
                {
                    parsedData.Summary = await GenerateSummaryFromCvData(parsedData);
                }

                return parsedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when analyzing CV with OpenAI");
                throw;
            }
        }

        private async Task<string> GenerateSummaryFromCvData(ParsedCvData cvData)
        {
            try
            {
                // Convert parsed CV data back to a structured text for the AI to work with
                StringBuilder cvSummary = new StringBuilder();
                cvSummary.AppendLine($"Name: {cvData.FirstName} {cvData.LastName}");

                if (!string.IsNullOrEmpty(cvData.JobPosition))
                    cvSummary.AppendLine($"Job Position: {cvData.JobPosition}");

                if (cvData.Skills?.Any() == true)
                {
                    cvSummary.AppendLine("Skills:");
                    foreach (var skill in cvData.Skills)
                    {
                        cvSummary.AppendLine($"- {skill.SkillName}");
                    }
                }

                if (cvData.Experiences?.Any() == true)
                {
                    cvSummary.AppendLine("Work Experience:");
                    foreach (var exp in cvData.Experiences)
                    {
                        cvSummary.AppendLine($"- {exp.JobPosition} at {exp.CompanyName} ({exp.StartedAt} - {exp.EndedAt})");
                    }
                }

                if (cvData.Educations?.Any() == true)
                {
                    cvSummary.AppendLine("Education:");
                    foreach (var edu in cvData.Educations)
                    {
                        cvSummary.AppendLine($"- {edu.Degree} in {edu.Major} from {edu.SchoolName}");
                    }
                }

                var requestBody = new
                {
                    model = "gpt-4",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are an AI specialized in writing professional summaries for job candidates. Based on the CV information provided, create a professional summary in 3-5 sentences that highlights the candidate's key strengths, career focus, years of experience if applicable, and their unique value proposition. The summary should be written in first person and sound professional but compelling."
                        },
                        new
                        {
                            role = "user",
                            content = $"Please generate a professional summary for my CV based on the following information:\n\n{cvSummary}"
                        }
                    },
                    temperature = 0.7,
                    max_tokens = 250
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                return responseObject.choices[0].message.content.ToString().Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary for CV");
                return "Experienced professional with a proven track record of success in the industry.";
            }
        }
    }
}   
