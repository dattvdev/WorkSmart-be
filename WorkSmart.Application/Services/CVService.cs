using AutoMapper;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class CVService 
    {
        private readonly ICVRepository _cvRepository;
        private readonly ICvParserService _cvParserService;
        private readonly IMapper _mapper;
        private readonly JobRecommendationService _recommendationService;
        public CVService(ICVRepository cvRepository
            , IMapper mapper
            , ICvParserService cvParserService
            , JobRecommendationService recommendationService)
        {
            _cvRepository = cvRepository;
            _mapper = mapper;
            _cvParserService = cvParserService;
            _recommendationService = recommendationService;
        }

        public async Task<CVDto> CreateCVAsync(CVDto cvDto)
        {
            var cv = _mapper.Map<CV>(cvDto);
            await _cvRepository.Add(cv);
            return _mapper.Map<CVDto>(cv);
        }

        public async Task<CVDto> GetCVByIdAsync(int id)
        {
            var cv = await _cvRepository.GetCVWithDetails(id);
            return _mapper.Map<CVDto>(cv);
        }

        public async Task<IEnumerable<CVDto>> GetAllCVsAsync(int userId)
        {
            var cvs = await _cvRepository.GetAllCVsByUserId(userId);
            var result = _mapper.Map<IEnumerable<CVDto>>(cvs);
            return result;
        }
        
        public async Task<CVDto> UpdateCVAsync(int userId, CVDto cvDto)
        {
            var existingCv = await _cvRepository.GetCVWithDetails(cvDto.CVID);

            if (existingCv == null)
            {
                throw new KeyNotFoundException($"CV with ID {cvDto.CVID} not found.");
            }
            

            if (existingCv.UserID != cvDto.UserID)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this CV.");
            }

            
            _mapper.Map(cvDto,existingCv); // Ánh xạ tất cả thuộc tính từ DTO vào entity
            existingCv.UpdatedAt = DateTime.Now;
            _cvRepository.Update(existingCv);

            // ✅ Cập nhật lại embedding vector
            await _recommendationService.DeleteCVEmbedding(existingCv.CVID);
            _recommendationService.ClearCVRecommendationCache(existingCv.CVID);

            return _mapper.Map<CVDto>(existingCv);  // Trả về CV được ánh xạ trở lại DTO
        }

        public async Task DeleteCVAsync(int id)
        {
            await _cvRepository.Delete(id); 
        }

        public void SetFeature(int cvId, int userId)
        {
            _cvRepository.SetFeature(cvId, userId);
        }

        public async Task<CVDto> UploadCVAsync(string filePath, int userId, string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentException("Đường dẫn file không được để trống", nameof(filePath));
                }

                //// Sử dụng CvParserService để phân tích CV
                //var cvSections = _cvParserService.ExtractCvSections(filePath);

                //cvSections = cvSections.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                //                     .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // Tạo CV entity mới
                var cv = new CV
                {
                    UserID = userId,
                    FilePath = filePath,
                    FileName = fileName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                //// Điền thông tin Summary nếu có
                //if (cvSections.ContainsKey("Summary"))
                //{
                //    cv.Summary = cvSections["Summary"];
                //}

                //// Điền thông tin Personal nếu có
                //if (cvSections.ContainsKey("Personal"))
                //{
                //    // Phân tích thông tin cá nhân
                //    ExtractPersonalInfo(cvSections["Personal"], cv);
                //}

                //// Khởi tạo các collections để tránh null
                //cv.Skills = new List<CV_Skill>();
                //cv.Experiences = new List<CV_Experience>();
                //cv.Educations = new List<CV_Education>();
                //cv.Certifications = new List<CV_Certification>();

                //// Thêm thông tin Skills
                //if (cvSections.ContainsKey("Skills") && !string.IsNullOrEmpty(cvSections["Skills"]))
                //{
                //    var skillsText = cvSections["Skills"];
                //    var skillsList = ParseSkills(skillsText);

                //    foreach (var skill in skillsList)
                //    {
                //        cv.Skills.Add(new CV_Skill
                //        {
                //            SkillName = skill,
                //        });
                //    }
                //}

                //// Thêm thông tin Experiences
                //if (cvSections.ContainsKey("Experience") && !string.IsNullOrEmpty(cvSections["Experience"]))
                //{
                //    var experienceText = cvSections["Experience"];
                //    var experiencesList = ParseExperiences(experienceText);

                //    foreach (var exp in experiencesList)
                //    {
                //        cv.Experiences.Add(new CV_Experience
                //        {
                //            Description = exp,
                //        });
                //    }
                //}

                //// Thêm thông tin Education
                //if (cvSections.ContainsKey("Education") && !string.IsNullOrEmpty(cvSections["Education"]))
                //{
                //    var educationText = cvSections["Education"];
                //    var educationsList = ParseEducations(educationText);

                //    foreach (var edu in educationsList)
                //    {
                //        cv.Educations.Add(new CV_Education
                //        {
                //            Description = edu,
                //        });
                //    }
                //}

                //// Thêm thông tin Certifications
                //if (cvSections.ContainsKey("Certifications") && !string.IsNullOrEmpty(cvSections["Certifications"]))
                //{
                //    var certText = cvSections["Certifications"];
                //    var certsList = ParseCertifications(certText);

                //    foreach (var cert in certsList)
                //    {
                //        cv.Certifications.Add(new CV_Certification
                //        {
                //            Description = cert,
                //        });
                //    }
                //}

                // Lưu CV vào database sử dụng phương thức Add và Save
                await _cvRepository.Add(cv);
                await _cvRepository.Save();

                // Map lại từ entity sang DTO để trả về
                var cvDto = _mapper.Map<CVDto>(cv);

                return cvDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi upload CV: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }

        private void ExtractPersonalInfo(string personalInfo, CV cv)
        {
            // Tìm email bằng regex
            var emailMatch = Regex.Match(personalInfo, @"[\w\.-]+@[\w\.-]+\.\w+");
            if (emailMatch.Success)
            {
                cv.Email = emailMatch.Value;
            }

            // Tìm số điện thoại bằng regex
            var phoneMatch = Regex.Match(personalInfo, @"(\+\d{1,3}[\s-]?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}");
            if (phoneMatch.Success)
            {
                cv.Phone = phoneMatch.Value;
            }

            // Tìm tên người dùng
            var nameMatch = Regex.Match(personalInfo, @"(?:Name|Full Name|Họ và tên):\s*([\p{L}\'\-\s]+)", RegexOptions.IgnoreCase);
            if (nameMatch.Success && nameMatch.Groups.Count > 1)
            {
                string fullName = nameMatch.Groups[1].Value.Trim();
                string[] nameParts = fullName.Split(' ');
                if (nameParts.Length > 0)
                {
                    cv.FirstName = nameParts[0];
                    if (nameParts.Length > 1)
                    {
                        cv.LastName = string.Join(" ", nameParts.Skip(1));
                    }
                }
            }

            // Tìm địa chỉ
            var addressMatch = Regex.Match(personalInfo, @"(?:Address|Địa chỉ):\s*([^\n]+)");
            if (addressMatch.Success && addressMatch.Groups.Count > 1)
            {
                cv.Address = addressMatch.Groups[1].Value.Trim();
            }

            // Tìm vị trí công việc
            var positionMatch = Regex.Match(personalInfo, @"(?:Position|Job Title|Title|Vị trí):\s*([^\n]+)");
            if (positionMatch.Success && positionMatch.Groups.Count > 1)
            {
                cv.JobPosition = positionMatch.Groups[1].Value.Trim();
            }
        }

        private List<string> ParseSkills(string skillsText)
        {
            // Xử lý text để tách thành danh sách kỹ năng
            var skills = new List<string>();

            // Phân tách theo dấu gạch đầu dòng hoặc dấu phẩy
            var skillLines = skillsText.Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in skillLines)
            {
                string cleanedLine = line.Trim();
                if (!string.IsNullOrEmpty(cleanedLine))
                {
                    // Loại bỏ dấu gạch đầu dòng nếu có
                    cleanedLine = Regex.Replace(cleanedLine, @"^[-•⚫●○⚪⚫★☆♦◆■□➢➤➥➔→▶►]\s*", "");
                    if (!string.IsNullOrEmpty(cleanedLine))
                    {
                        skills.Add(cleanedLine);
                    }
                }
            }

            return skills;
        }

        private List<string> ParseExperiences(string experienceText)
        {
            var experiences = new List<string>();

            // Biểu thức chính quy để tách từng phần kinh nghiệm dựa trên định dạng thông thường
            var expRegex = new Regex(@"(?:[-•]\s*)?(.+?\(\d{4}\))", RegexOptions.Multiline);

            var matches = expRegex.Split(experienceText)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            if (matches.Count <= 1)
            {
                experiences.Add(experienceText.Trim());
            }
            else
            {
                experiences.AddRange(matches);
            }

            return experiences;
        }

        private List<string> ParseEducations(string eduText)
        {
            var educations = new List<string>();

            // Regex nhận diện thông tin học vấn với dấu "–", "-", hoặc dấu phẩy ","
            var eduRegex = new Regex(@"(.+?)\s*[-–]\s*(.+)", RegexOptions.Multiline);

            var matches = eduRegex.Matches(eduText);

            foreach (Match match in matches)
            {
                string cleanedEdu = match.Groups[0].Value.Trim();
                if (!string.IsNullOrEmpty(cleanedEdu))
                {
                    educations.Add(cleanedEdu);
                }
            }

            // Nếu không tìm thấy dữ liệu, giữ nguyên nội dung gốc
            if (educations.Count == 0)
            {
                educations.Add(eduText.Trim());
            }

            return educations;
        }

        private List<string> ParseCertifications(string certText)
        {
            var certifications = new List<string>();

            // Regex nhận diện các chứng chỉ theo danh sách gạch đầu dòng hoặc câu văn có năm (202X)
            var certRegex = new Regex(@"(?:[-•]\s*)?(.+?\(\d{4}\))", RegexOptions.Multiline);

            var matches = certRegex.Matches(certText);

            foreach (Match match in matches)
            {
                string cleanedCert = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(cleanedCert))
                {
                    certifications.Add(cleanedCert);
                }
            }

            // Nếu không tìm thấy chứng chỉ nào, giữ nguyên nội dung gốc
            if (certifications.Count == 0)
            {
                certifications.Add(certText.Trim());
            }

            return certifications;
        }

        public void HideCV(int cvId)
        {
            _cvRepository.HideCV(cvId);
        }

        public async Task<CVCreationLimitDto> GetRemainingCVCreationLimit(int userID)
        {
            return await _cvRepository.GetRemainingCVCreationLimit(userID);
        }
    }
}