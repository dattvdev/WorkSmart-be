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
You are an AI specialized in CV analysis. Below is the content of a CV that may have irregular formatting due to PDF extraction. Your job is to carefully analyze the content and extract structured information, even if the formatting is not perfect.

First, identify clear sections like Contact Details, Education, Skills, Experience, etc. Then, extract all information available in each section.

Return the result in JSON format as follows:
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
1. Look for contact information near the top of the document, including email, phone, and location
2. Split the full name into firstName and lastName
3. Look for section headers that might indicate different parts of the CV (Experience, Education, etc.)
4. Time fields should be returned in month/year or day/month/year format if possible
5. Extract workType (such as Full-time, Part-time, Remote, etc.) if available
6. The job position might be found near the name or in a separate professional title section
7. For each skill, separate the skill name from its proficiency level (if available)
8. If a summary section exists, use that directly; otherwise do not generate one at this stage
9. Return empty array [] for sections that don't have any items
10. Return empty string """" for fields where information is not available
";
                var requestBody = new
                {
                    model = "gpt-4o-mini", // Or other suitable OpenAI model
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

                // Kiểm tra phản hồi HTTP
                if (!response.IsSuccessStatusCode)
                {
                    return CreateEmptyCvData();
                }

                var responseString = await response.Content.ReadAsStringAsync();

                try
                {
                    var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);

                    // Kiểm tra cấu trúc phản hồi
                    if (responseObject?.choices == null || responseObject.choices.Count == 0 ||
                        responseObject.choices[0]?.message?.content == null)
                    {
                        return CreateEmptyCvData();
                    }

                    string parsedCvJson = responseObject.choices[0].message.content.ToString();

                    if (parsedCvJson.StartsWith("```json"))
                    {
                        parsedCvJson = parsedCvJson.Replace("```json", "").Replace("```", "").Trim();
                    }

                    try
                    {
                        var parsedData = JsonConvert.DeserializeObject<ParsedCvData>(parsedCvJson);

                        if (parsedData == null)
                        {
                            return CreateEmptyCvData();
                        }

                        if (string.IsNullOrEmpty(parsedData.Summary))
                        {
                            try
                            {
                                parsedData.Summary = await GenerateSummaryFromCvData(parsedData);
                            }
                            catch (Exception summaryEx)
                            {
                                parsedData.Summary = "Professional with experience in the field.";
                            }
                        }

                        return parsedData;
                    }
                    catch (JsonReaderException jsonEx)
                    {
                        return CreateEmptyCvData();
                    }
                }
                catch (Exception parseEx)
                {
                    return CreateEmptyCvData();
                }
            }
            catch (Exception ex)
            {
                return CreateEmptyCvData();
            }
        }

        private ParsedCvData CreateEmptyCvData()
        {
            return new ParsedCvData
            {
                FirstName = "",
                LastName = "",
                JobPosition = "",
                WorkType = "",
                Summary = "Professional with relevant experience and skills.",
                Address = "",
                Phone = "",
                Email = "",
                Link = "",
                FileName = "",
                FilePath = "",
                Educations = new List<ParsedEducation>(),
                Experiences = new List<ParsedExperience>(),
                Certifications = new List<ParsedCertification>(),
                Skills = new List<ParsedSkill>()
            };
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
                    model = "gpt-4o-mini",
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
