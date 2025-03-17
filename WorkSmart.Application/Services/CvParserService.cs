using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkSmart.Application.Services
{
    public class CvParserService
    {
        public static Dictionary<string, string> ExtractCvSections(string filePath)
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

            File.WriteAllText("CvExtractedText.txt", content);

            // Loại bỏ ký tự Unicode đặc biệt, khoảng trắng thừa
            content = Regex.Replace(content, @"[\uE000-\uF8FF]", ""); // Loại bỏ ký tự đặc biệt
            content = Regex.Replace(content, @"\s{2,}", " "); // Chuẩn hóa khoảng trắng

            return ParseCvContent(content);
        }

        private static Dictionary<string, string> ParseCvContent(string content)
        {
            var sections = new Dictionary<string, string>();
            // Tạo map of common section title thường gặp
            var sectionTitleVariations = new Dictionary<string, List<string>>
            {
                { "Skills", new List<string> { "Skills", "Technical Skills", "Software", "Professional Skills", "Key Skills", "Core Skills", "Skill Set" } },
                { "Experience", new List<string> { "Experience", "Work Experience", "Employment History", "Career History", "Work History" } },
                { "Education", new List<string> { "Education", "Academic Background", "Educational Background", "Academic Qualifications", "Qualifications" } },
                { "Certifications", new List<string> { "Certifications", "Certificates", "Professional Certifications", "Credentials" } },
                { "Projects", new List<string> { "Projects", "Project", "Personal Projects", "Professional Projects", "Key Projects", "Recent Projects", "PROJECT", "PROJECT AND PRACTICE" } },
                { "Summary", new List<string> { "Professional Summary", "Summary", "Profile", "About Me", "Career Objective", "Objective" } },
                { "Personal", new List<string> { "Personal Information", "Personal Details", "Contact Information", "Contact Details" } }
            };
                    // Tạo flattened list cho tìm kiếm regex
            var allTitles = sectionTitleVariations.Values.SelectMany(x => x).ToList();
            foreach (var key in sectionTitleVariations.Keys)
            {
                sections[key] = "";
            }

            // Phương pháp 1: Sử dụng regex gốc với danh sách tiêu đề mở rộng
            string pattern = @"(?<title>" + string.Join("|", allTitles.Select(Regex.Escape)) + @")\s*[:.]?\s*(?:\n|\r\n)?(?<content>(?:.|\n)*?)(?=\n\s*(?:" + string.Join("|", allTitles.Select(t => Regex.Escape(t))) + @")\s*[:.]?|$)";

            // Phương pháp 2: Regex bổ sung để nhận diện các tiêu đề dựa trên định dạng (viết hoa hoặc số)
            // Đây là mẫu regex thay thế nhận diện tiêu đề dựa trên định dạng 
            string formatBasedPattern = @"(?<title>(?:" + string.Join("|", allTitles.Select(Regex.Escape)) + @")|\n[A-Z][A-Z\s]+(?:\s*&\s*[A-Z][A-Z\s]+)?|\n\d+[A-Z][A-Z\s]+)\s*[:.]?\s*(?:\n|\r\n)?(?<content>(?:.|\n)*?)(?=\n(?:[A-Z][A-Z\s]+(?:\s*&\s*[A-Z][A-Z\s]+)?|\d+[A-Z][A-Z\s]+)|(?:" + string.Join("|", allTitles.Select(t => Regex.Escape(t))) + @")|\Z)";

            // Sử dụng mẫu regex đã chọn
            var matches = Regex.Matches(content, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                string matchedTitle = match.Groups["title"].Value.Trim();
                string sectionContent = match.Groups["content"].Value.Trim();

                // Standardize the section key
                string standardKey = sectionTitleVariations
                    .FirstOrDefault(kvp => kvp.Value.Any(v => v.Equals(matchedTitle, StringComparison.OrdinalIgnoreCase)))
                    .Key;

                if (!string.IsNullOrEmpty(standardKey))
                {
                    // Xử lý các dấu đầu dòng
                    sectionContent = Regex.Replace(sectionContent, @"(\n\s*[-•⚫●○⚪⚫★☆♦◆■□➢➤➥➔→▶►])", "\n-");
                    // Chuẩn hóa khoảng trắng
                    sectionContent = Regex.Replace(sectionContent, @"\s{2,}", " ");
                    sections[standardKey] = sectionContent;
                }
            }

            return sections;
        }
    }
}
