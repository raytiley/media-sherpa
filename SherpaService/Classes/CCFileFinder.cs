using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Sherpa.Classes
{
    public class CCFileFinder
    {
        public static string FindFile(int showID, IEnumerable<string> searchPaths)
        {
            foreach (var searchPath in searchPaths)
            {
                var files = Directory.GetFiles(searchPath);
                var showIDRegex = new Regex(@"^(\d+)-?.+[\.mpeg|\.mov|\.mpg|\.mp4]$");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var match = showIDRegex.Match(fileName.ToLower());
                    if (match.Success)
                    {
                        var fileShowID = Int32.Parse(match.Groups[1].ToString());
                        if (showID == fileShowID)
                            return file;
                    }
                }
            }

            return String.Empty;
        }
    }
}
