using System.Collections.Generic;

namespace ForbiddenWordsScanner
{
    public class ScanOptions
    {
        public string SourcePath { get; set; }
        public string OutputDirectory { get; set; }
        public HashSet<string> ForbiddenWords { get; set; } = new HashSet<string>();
    }

    public class FileScanResult
    {
        public string OriginalPath { get; set; }
        public string CopiedPath { get; set; }
        public string CleanedPath { get; set; }
        public long FileSizeBytes { get; set; }
        public int TotalReplacements { get; set; }
        public Dictionary<string, int> WordCounts { get; set; } = new Dictionary<string, int>();
    }

    public class ScanSummary
    {
        public int TotalFilesScanned { get; set; }
        public int FilesWithForbiddenWords { get; set; }
        public List<FileScanResult> MatchedFiles { get; set; } = new List<FileScanResult>();
        public Dictionary<string, int> GlobalWordStats { get; set; } = new Dictionary<string, int>();
        public string ReportFilePath { get; set; }
    }
}
