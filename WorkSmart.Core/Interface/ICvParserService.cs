using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Interface
{
    public interface ICvParserService
    {
        Dictionary<string, string> ExtractCvSections(string filePath);
    }
}
