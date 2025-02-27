using System.Text.Json.Serialization;

namespace FixMessageAnalyzer.Data.DTOs
{
    public class FileUploadConfigDto
    {
        [JsonPropertyName("directory")]
        public string Directory { get; set; }
        [JsonPropertyName("pattern")]
        public string Pattern { get; set; }
        [JsonPropertyName("scanIntervalMs")]
        public int ScanIntervalMs { get; set; }
        [JsonPropertyName("processSubDirectories")]
        public bool ProcessSubDirectories { get; set; }
        [JsonPropertyName("fileNameDatePattern")]
        public string FileNameDatePattern { get; set; }
    }
}
