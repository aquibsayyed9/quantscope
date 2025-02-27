using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace FixMessageAnalyzer.Core.Services
{
    public interface IFixFieldMapperService
    {
        string GetFieldName(string tag);
        bool HasMappings();
        int MappingCount();
    }
}

namespace FixMessageAnalyzer.Core.Services
{
    public class FixFieldMapper : IFixFieldMapperService
    {
        private readonly Dictionary<string, string> _fieldMappings;
        private readonly ILogger<FixFieldMapper> _logger;

        public FixFieldMapper(ILogger<FixFieldMapper> logger)
        {
            _logger = logger;
            _fieldMappings = LoadFieldMappings();
        }

        // Add parameterless constructor for cases where we can't use DI
        public FixFieldMapper() : this(new LoggerFactory().CreateLogger<FixFieldMapper>())
        {
        }

        private Dictionary<string, string> LoadFieldMappings()
        {
            var mappings = new Dictionary<string, string>();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string xmlFilePath = Path.Combine(baseDirectory, "FixSpec", "FIX50SP2.xml");

            _logger.LogInformation($"Loading FIX field mappings from: {xmlFilePath}");

            try
            {
                if (!File.Exists(xmlFilePath))
                {
                    _logger.LogWarning($"FIX specification file not found at: {xmlFilePath}. Checking parent directories...");

                    // Try to find the file in parent directories (useful during development)
                    var currentDir = new DirectoryInfo(baseDirectory);
                    while (currentDir != null && !File.Exists(xmlFilePath))
                    {
                        currentDir = currentDir.Parent;
                        if (currentDir != null)
                        {
                            xmlFilePath = Path.Combine(currentDir.FullName, "FixSpec", "FIX50SP2.xml");
                            _logger.LogInformation($"Trying path: {xmlFilePath}");
                        }
                    }
                }

                if (!File.Exists(xmlFilePath))
                {
                    _logger.LogError("Could not find FIX specification file in any parent directory");
                    return mappings;
                }

                var doc = XDocument.Load(xmlFilePath);
                foreach (var field in doc.Descendants("field"))
                {
                    string number = field.Attribute("number")?.Value ?? "";
                    string name = field.Attribute("name")?.Value ?? "";

                    if (!string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(name))
                    {
                        mappings[number] = name;
                        _logger.LogDebug($"Mapped FIX tag {number} to {name}");
                    }
                }

                _logger.LogInformation($"Successfully loaded {mappings.Count} FIX field mappings");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading FIX field mappings");
            }

            return mappings;
        }

        public string GetFieldName(string tag)
        {
            if (_fieldMappings.TryGetValue(tag, out var fieldName))
            {
                return fieldName;
            }

            _logger.LogDebug($"No mapping found for FIX tag: {tag}");
            return tag;
        }

        // Helper method to check if mappings were loaded successfully
        public bool HasMappings()
        {
            return _fieldMappings.Count > 0;
        }

        // Helper method to get total number of mappings
        public int MappingCount()
        {
            return _fieldMappings.Count;
        }
    }
}