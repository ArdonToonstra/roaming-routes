using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using RoamingRoutes.Shared.Models.Games;

namespace RoamingRoutes.Services
{
    public interface IWordPairService
    {
        List<WordPairCategory> GetCategories();
        WordPairCategory? GetCategoryByName(string name);
        WordPair GetRandomPairFromCategory(string categoryName);
    }

    public class WordPairService : IWordPairService
    {
        private readonly WordPairConfiguration _configuration;
        private readonly ILogger<WordPairService> _logger;

        public WordPairService(ILogger<WordPairService> logger)
        {
            _logger = logger;
            _configuration = LoadWordPairs();
        }

        private WordPairConfiguration LoadWordPairs()
        {
            try
            {
                var yamlPath = Path.Combine("Data", "WordPairs.yaml");
                
                if (!File.Exists(yamlPath))
                {
                    _logger.LogWarning("WordPairs.yaml not found at {Path}, using default word pairs", yamlPath);
                    return CreateDefaultConfiguration();
                }

                var yamlContent = File.ReadAllText(yamlPath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var config = deserializer.Deserialize<WordPairConfiguration>(yamlContent);
                
                _logger.LogInformation("Loaded {CategoryCount} word pair categories from {Path}", 
                    config.Categories.Count, yamlPath);

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load word pairs from YAML, using default configuration");
                return CreateDefaultConfiguration();
            }
        }

        private WordPairConfiguration CreateDefaultConfiguration()
        {
            // Fallback to hardcoded word pairs if YAML loading fails
            return new WordPairConfiguration
            {
                Categories = new List<WordPairCategory>
                {
                    new WordPairCategory
                    {
                        Name = "Everyday Words",
                        Description = "Common everyday objects and concepts",
                        Pairs = new List<WordPair>
                        {
                            new WordPair { Civilian = "Coffee", Undercover = "Tea" },
                            new WordPair { Civilian = "Dog", Undercover = "Cat" },
                            new WordPair { Civilian = "Car", Undercover = "Bus" },
                            new WordPair { Civilian = "Apple", Undercover = "Orange" },
                            new WordPair { Civilian = "Summer", Undercover = "Winter" }
                        }
                    }
                }
            };
        }

        public List<WordPairCategory> GetCategories()
        {
            return _configuration.Categories;
        }

        public WordPairCategory? GetCategoryByName(string name)
        {
            return _configuration.Categories.FirstOrDefault(c => 
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public WordPair GetRandomPairFromCategory(string categoryName)
        {
            var category = GetCategoryByName(categoryName);
            if (category == null || !category.Pairs.Any())
            {
                throw new ArgumentException($"Category '{categoryName}' not found or has no word pairs");
            }

            var random = new Random();
            return category.Pairs[random.Next(category.Pairs.Count)];
        }
    }
}
