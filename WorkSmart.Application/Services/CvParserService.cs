using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkSmart.Core.Dto;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository;

namespace WorkSmart.Application.Services
{
    public class CvParserService : ICvParserService
    {
        private readonly OpenAIService _openAiService;
        private readonly ILogger<CvParserService> _logger;
        private readonly WorksmartDBContext _dbContext;

        public CvParserService(OpenAIService openAiService, ILogger<CvParserService> logger, WorksmartDBContext dbContext)
        {
            _openAiService = openAiService;
            _logger = logger;
            _dbContext = dbContext;
        }

        public Dictionary<string, string> ExtractCvSections(string filePath)
        {
            StringBuilder text = new StringBuilder();

            using (PdfReader reader = new PdfReader(filePath))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                }
            }

            string content = text.ToString();

            // Loại bỏ ký tự Unicode đặc biệt, khoảng trắng thừa
            content = Regex.Replace(content, @"[\uE000-\uF8FF]", ""); // Loại bỏ ký tự đặc biệt
            content = Regex.Replace(content, @"\s{2,}", " "); // Chuẩn hóa khoảng trắng

            return ParseCvContent(content);
        }

        private Dictionary<string, string> ParseCvContent(string content)
        {
            var sections = new Dictionary<string, string>();
            var sectionTitleVariations = new Dictionary<string, List<string>>
            {
                { "Skills", new List<string> { "Skills", "Technical Skills", "Software", "Professional Skills", "Key Skills", "Core Skills", "Skill Set" } },
                { "Experience", new List<string> { "Experience", "Work Experience", "Employment History", "Career History", "Work History" } },
                { "Education", new List<string> { "Education", "Academic Background", "EDUCATIONAL BACKGROUND", "Academic Qualifications", "Qualifications" } },
                { "Certifications", new List<string> { "Certifications", "Certificate And Award", "Certificate", "Professional Certifications", "Credentials" } },
                { "Projects", new List<string> { "Projects", "Project", "Personal Projects", "Professional Projects", "Key Projects", "Recent Projects", "PROJECT", "PROJECT AND PRACTICE" } },
                { "Summary", new List<string> { "Professional Summary", "Summary", "Profile", "About Me", "Career Objective", "Objective" } },
                { "Personal", new List<string> { "Personal Information", "Personal Details", "Contact Information", "Contact Details" } }
            };

            var allTitles = sectionTitleVariations.Values.SelectMany(x => x).ToList();
            foreach (var key in sectionTitleVariations.Keys)
            {
                sections[key] = "";
            }

            string pattern = @"(?<title>" + string.Join("|", allTitles.Select(Regex.Escape)) + @")\s*[:.]?\s*(?:\n|\r\n)?(?<content>(?:.|\n)*?)(?=\n\s*(?:" + string.Join("|", allTitles.Select(t => Regex.Escape(t))) + @")\s*[:.]?|$)";
            var matches = Regex.Matches(content, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                string matchedTitle = match.Groups["title"].Value.Trim();
                string sectionContent = match.Groups["content"].Value.Trim();

                string standardKey = sectionTitleVariations
                    .FirstOrDefault(kvp => kvp.Value.Contains(matchedTitle, StringComparer.OrdinalIgnoreCase))
                    .Key;

                if (!string.IsNullOrEmpty(standardKey))
                {
                    sectionContent = Regex.Replace(sectionContent, @"(\n\s*[-•⚫●○⚪⚫★☆♦◆■□➢➤➥➔→▶►])", "\n-");
                    sectionContent = Regex.Replace(sectionContent, @"\s{2,}", " ");
                    sections[standardKey] = sectionContent;
                }
            }

            return sections;
        }

        public string ExtractCvContent(string filePath)
        {
            StringBuilder text = new StringBuilder();
            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                {
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        // Thêm dấu ngắt trang để phân biệt rõ giữa các trang
                        if (i > 1) text.AppendLine("\n--- PAGE BREAK ---\n");

                        // Sử dụng chiến lược trích xuất chi tiết hơn
                        var strategy = new SimpleTextExtractionStrategy();
                        string pageText = PdfTextExtractor.GetTextFromPage(reader, i, strategy);
                        text.AppendLine(pageText);
                    }
                }

                string content = text.ToString();

                content = Regex.Replace(content, @"[\uE000-\uF8FF]", ""); // Loại bỏ ký tự đặc biệt
                content = Regex.Replace(content, @"\r\n", "\n"); // Chuẩn hóa các dấu xuống dòng
                content = Regex.Replace(content, @"\n{3,}", "\n\n"); // Giảm số dòng trống liên tiếp


                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting content from PDF");
                throw;
            }
        }

        public async Task<ParsedCvData> ParseCvAsync(string cvContent, int userId, string filePath, string fileName)
        {
            try
            {
                // Gọi OpenAI để phân tích nội dung CV
                var parsedData = await _openAiService.ParseCvContentAsync(cvContent);

                parsedData.FilePath = filePath;
                parsedData.FileName = fileName;

                // Lưu dữ liệu vào cơ sở dữ liệu
                await SaveParsedCvToDatabase(parsedData, userId, filePath, fileName);

                return parsedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi phân tích CV");
                throw;
            }
        }

        private async Task<int> SaveParsedCvToDatabase(ParsedCvData parsedData, int userId, string filePath, string fileName)
        {
            // Tạo entity CV mới
            var cvEntity = new CV
            {
                UserID = userId,
                FileName = fileName,
                FilePath = filePath,
                FirstName = parsedData.FirstName,
                LastName = parsedData.LastName,
                JobPosition = parsedData.JobPosition,
                WorkType = parsedData.WorkType,
                Summary = parsedData.Summary,
                Address = parsedData.Address,
                Phone = parsedData.Phone,
                Email = parsedData.Email,
                Link = filePath,
                Title = fileName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsHidden = false
            };

            _dbContext.CVs.Add(cvEntity);
            await _dbContext.SaveChangesAsync(); // Lưu để có CVID

            // Thêm educations
            if (parsedData.Educations != null && parsedData.Educations.Any())
            {
                foreach (var edu in parsedData.Educations)
                {
                    _dbContext.CV_Educations.Add(new CV_Education
                    {
                        CVID = cvEntity.CVID,
                        Major = edu.Major,
                        SchoolName = edu.SchoolName,
                        Degree = edu.Degree,
                        Description = edu.Description,
                        StartedAt = ParseDateTime(edu.StartedAt),
                        EndedAt = ParseDateTime(edu.EndedAt)
                    });
                }
            }

            // Thêm experiences
            if (parsedData.Experiences != null && parsedData.Experiences.Any())
            {
                foreach (var exp in parsedData.Experiences)
                {
                    _dbContext.CV_Experiences.Add(new CV_Experience
                    {
                        CVID = cvEntity.CVID,
                        JobPosition = exp.JobPosition,
                        CompanyName = exp.CompanyName,
                        Address = exp.Address,
                        Description = exp.Description,
                        StartedAt = ParseDateTime(exp.StartedAt),
                        EndedAt = ParseDateTime(exp.EndedAt)
                    });
                }
            }

            // Thêm certifications
            if (parsedData.Certifications != null && parsedData.Certifications.Any())
            {
                foreach (var cert in parsedData.Certifications)
                {
                    _dbContext.CV_Certifications.Add(new CV_Certification
                    {
                        CVID = cvEntity.CVID,
                        CertificateName = cert.CertificateName,
                        Description = cert.Description,
                        CreateAt = ParseDateTime(cert.CreateAt)
                    });
                }
            }

            // Thêm skills
            if (parsedData.Skills != null && parsedData.Skills.Any())
            {
                foreach (var skill in parsedData.Skills)
                {
                    _dbContext.CV_Skills.Add(new CV_Skill
                    {
                        CVID = cvEntity.CVID,
                        SkillName = skill.SkillName,
                        Description = skill.Description
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
            return cvEntity.CVID;
        }

        private DateTime? ParseDateTime(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return null;

            // Thử parse các định dạng phổ biến
            string[] formats = { "MM/yyyy", "dd/MM/yyyy", "MM-yyyy", "dd-MM-yyyy", "yyyy-MM-dd", "yyyy/MM/dd" };

            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            // Thử parse với cách khác
            if (DateTime.TryParse(dateString, out result))
            {
                return result;
            }

            return null;
        }
    }
}
