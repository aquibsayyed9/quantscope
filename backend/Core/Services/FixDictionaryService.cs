using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FixMessageAnalyzer.Core.Services
{
    public interface IFixDictionaryService
    {
        Task InitializeDictionariesAsync();
        string GetFieldName(string tag, string version = null);
        string GetMessageTypeName(string msgType, string version = null);
        bool IsFieldRequired(string tag, string msgType, string version = null);
        IEnumerable<string> GetSupportedVersions();
        string DetectVersion(Dictionary<string, string> fields);
        bool IsValidValue(string tag, string value, string version = null);
    }

    public class FixDictionaryService : IFixDictionaryService
    {
        private readonly ConcurrentDictionary<string, FixDictionary> _dictionaries = new();
        private readonly ILogger<FixDictionaryService> _logger;
        private string _defaultVersion = "FIX.4.4"; // Fallback default

        private class FixDictionary
        {
            public string Version { get; set; }
            public Dictionary<string, string> FieldNames { get; } = new();
            public Dictionary<string, string> MessageTypes { get; } = new();
            public Dictionary<string, HashSet<string>> RequiredFields { get; } = new();
            public Dictionary<string, Dictionary<string, HashSet<string>>> ValidEnumValues { get; } = new();
        }

        public FixDictionaryService(ILogger<FixDictionaryService> logger)
        {
            _logger = logger;
        }

        public async Task InitializeDictionariesAsync()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fixSpecDirectory = Path.Combine(baseDirectory, "FixSpec");

            // Try to find the spec directory in parent folders if not found
            if (!Directory.Exists(fixSpecDirectory))
            {
                var currentDir = new DirectoryInfo(baseDirectory);
                while (currentDir != null && !Directory.Exists(fixSpecDirectory))
                {
                    currentDir = currentDir.Parent;
                    if (currentDir != null)
                    {
                        fixSpecDirectory = Path.Combine(currentDir.FullName, "FixSpec");
                    }
                }
            }

            if (!Directory.Exists(fixSpecDirectory))
            {
                _logger.LogError("FIX specification directory not found");
                return;
            }

            foreach (var filePath in Directory.GetFiles(fixSpecDirectory, "*.xml"))
            {
                try
                {
                    await Task.Run(() => LoadFixDictionary(filePath));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading FIX dictionary from {FilePath}", filePath);
                }
            }

            _logger.LogInformation("Loaded {Count} FIX dictionaries", _dictionaries.Count);
        }

        private void LoadFixDictionary(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            _logger.LogInformation("Loading FIX dictionary from {FilePath}", filePath);

            try
            {
                var dictionary = new FixDictionary();
                var doc = XDocument.Load(filePath);

                // Extract version from the file
                var fixElement = doc.Element("fix");
                dictionary.Version = fixElement?.Attribute("type")?.Value ?? fileName;

                // Load fields
                foreach (var field in doc.Descendants("field"))
                {
                    string number = field.Attribute("number")?.Value ?? "";
                    string name = field.Attribute("name")?.Value ?? "";

                    if (!string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(name))
                    {
                        dictionary.FieldNames[number] = name;

                        // Load enum values if present
                        var enumValues = new HashSet<string>();
                        foreach (var value in field.Elements("value"))
                        {
                            string enumValue = value.Attribute("enum")?.Value ?? "";
                            if (!string.IsNullOrEmpty(enumValue))
                            {
                                enumValues.Add(enumValue);
                            }
                        }

                        if (enumValues.Count > 0)
                        {
                            if (!dictionary.ValidEnumValues.ContainsKey(number))
                            {
                                dictionary.ValidEnumValues[number] = new Dictionary<string, HashSet<string>>();
                            }
                            dictionary.ValidEnumValues[number]["*"] = enumValues;
                        }
                    }
                }

                // Load message types
                foreach (var message in doc.Descendants("message"))
                {
                    string msgType = message.Attribute("msgtype")?.Value ?? "";
                    string name = message.Attribute("name")?.Value ?? "";

                    if (!string.IsNullOrEmpty(msgType) && !string.IsNullOrEmpty(name))
                    {
                        dictionary.MessageTypes[msgType] = name;

                        // Load required fields for this message type
                        var requiredFields = new HashSet<string>();
                        foreach (var field in message.Elements("field"))
                        {
                            bool required = field.Attribute("required")?.Value == "Y";
                            string number = field.Attribute("number")?.Value ?? "";

                            if (required && !string.IsNullOrEmpty(number))
                            {
                                requiredFields.Add(number);
                            }
                        }

                        if (requiredFields.Count > 0)
                        {
                            dictionary.RequiredFields[msgType] = requiredFields;
                        }
                    }
                }

                _dictionaries[dictionary.Version] = dictionary;
                _logger.LogInformation("Successfully loaded FIX dictionary for version {Version}", dictionary.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing FIX specification file {FilePath}", filePath);
            }
        }

        public string GetFieldName(string tag, string version = null)
        {
            version ??= _defaultVersion;

            if (_dictionaries.TryGetValue(version, out var dictionary) &&
                dictionary.FieldNames.TryGetValue(tag, out var fieldName))
            {
                return fieldName;
            }

            // Fallback to any dictionary that has this field
            foreach (var dict in _dictionaries.Values)
            {
                if (dict.FieldNames.TryGetValue(tag, out var name))
                {
                    return name;
                }
            }

            return tag;
        }

        public string GetMessageTypeName(string msgType, string version = null)
        {
            version ??= _defaultVersion;

            if (_dictionaries.TryGetValue(version, out var dictionary) &&
                dictionary.MessageTypes.TryGetValue(msgType, out var msgName))
            {
                return msgName;
            }

            // Fallback to any dictionary
            foreach (var dict in _dictionaries.Values)
            {
                if (dict.MessageTypes.TryGetValue(msgType, out var name))
                {
                    return name;
                }
            }

            return $"Unknown ({msgType})";
        }

        public bool IsFieldRequired(string tag, string msgType, string version = null)
        {
            version ??= _defaultVersion;

            if (_dictionaries.TryGetValue(version, out var dictionary) &&
                dictionary.RequiredFields.TryGetValue(msgType, out var requiredFields))
            {
                return requiredFields.Contains(tag);
            }

            return false;
        }

        public IEnumerable<string> GetSupportedVersions()
        {
            return _dictionaries.Keys;
        }

        public string DetectVersion(Dictionary<string, string> fields)
        {
            // Check for BeginString (tag 8)
            if (fields.TryGetValue("8", out var beginString))
            {
                if (beginString == "FIXT.1.1")
                {
                    // For FIXT.1.1, check for DefaultApplVerID (1137) which is required in Logon
                    if (fields.TryGetValue("1137", out var defaultApplVerID))
                    {
                        return MapApplVerIDToVersion(defaultApplVerID);
                    }
                    // Fall back to ApplVerID if present
                    else if (fields.TryGetValue("1128", out var applVerID))
                    {
                        return MapApplVerIDToVersion(applVerID);
                    }
                    _logger.LogWarning("FIXT.1.1 message without DefaultApplVerID or ApplVerID, using default");
                }
                // Check if this is a version we support
                else if (_dictionaries.ContainsKey(beginString))
                {
                    return beginString;
                }
                _logger.LogWarning("Unknown FIX version: {Version}, using default", beginString);
            }
            return _defaultVersion;
        }

        public bool IsValidValue(string tag, string value, string version = null)
        {
            version ??= _defaultVersion;

            if (_dictionaries.TryGetValue(version, out var dictionary) &&
                dictionary.ValidEnumValues.TryGetValue(tag, out var contextEnums))
            {
                if (contextEnums.TryGetValue("*", out var validValues))
                {
                    return validValues.Contains(value);
                }
            }

            // If no enumeration defined, consider it valid
            return true;
        }

        private string MapApplVerIDToVersion(string applVerID)
        {
            return applVerID switch
            {
                "4" => "FIX.4.2",
                "5" => "FIX.4.3",
                "6" => "FIX.4.4",
                "7" => "FIX.5.0",
                "8" => "FIX.5.0SP1",
                "9" => "FIX.5.0SP2",
                _ => _defaultVersion
            };
        }
    }
}