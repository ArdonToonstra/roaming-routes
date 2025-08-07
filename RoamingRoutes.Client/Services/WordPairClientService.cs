using RoamingRoutes.Shared.Models.Games;
using System.Net.Http.Json;

namespace RoamingRoutes.Client.Services
{
    public interface IWordPairClientService
    {
        Task<List<WordPairCategory>> GetCategoriesAsync();
        Task<WordPair> GetRandomPairFromCategoryAsync(string categoryName);
    }

    public class WordPairClientService : IWordPairClientService
    {
        private readonly HttpClient _httpClient;

        public WordPairClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WordPairCategory>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _httpClient.GetFromJsonAsync<List<WordPairCategory>>("/api/wordpairs/categories");
                return categories ?? new List<WordPairCategory>();
            }
            catch (Exception)
            {
                // Return default categories if API call fails
                return GetDefaultCategories();
            }
        }

        public async Task<WordPair> GetRandomPairFromCategoryAsync(string categoryName)
        {
            try
            {
                var pair = await _httpClient.GetFromJsonAsync<WordPair>($"/api/wordpairs/categories/{Uri.EscapeDataString(categoryName)}/random");
                return pair ?? GetDefaultPair();
            }
            catch (Exception)
            {
                // Return default pair if API call fails
                return GetDefaultPair();
            }
        }

        private List<WordPairCategory> GetDefaultCategories()
        {
            return new List<WordPairCategory>
            {
                new WordPairCategory
                {
                    Name = "Everyday Words",
                    Description = "Common everyday objects and concepts",
                    Pairs = new List<WordPair>
                    {
                        new WordPair { Civilian = "Coffee", Undercover = "Tea" },
                        new WordPair { Civilian = "Dog", Undercover = "Cat" },
                        new WordPair { Civilian = "Car", Undercover = "Bus" }
                    }
                }
            };
        }

        private WordPair GetDefaultPair()
        {
            return new WordPair { Civilian = "Coffee", Undercover = "Tea" };
        }
    }
}
