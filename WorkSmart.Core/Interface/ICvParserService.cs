using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto;

namespace WorkSmart.Core.Interface
{
    public interface ICvParserService
    {
        Dictionary<string, string> ExtractCvSections(string filePath);

        string ExtractCvContent(string filePath);

        Task<ParsedCvData> ParseCvAsync(string cvContent, int userId, string filePath, string fileName);
    }
}
