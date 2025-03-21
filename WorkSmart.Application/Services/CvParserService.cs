using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class CvParserService : ICvParserService
    {
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
    }

}
