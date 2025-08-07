using RoamingRoutes.Shared.Models.Games;
using RoamingRoutes.Services;
using RoamingRoutes.Client.Services;

namespace RoamingRoutes.Services
{
    public class ServerSideWordPairClientService : IWordPairClientService
    {
        private readonly IWordPairService _wordPairService;

        public ServerSideWordPairClientService(IWordPairService wordPairService)
        {
            _wordPairService = wordPairService;
        }

        public Task<List<WordPairCategory>> GetCategoriesAsync()
        {
            var categories = _wordPairService.GetCategories();
            return Task.FromResult(categories);
        }

        public Task<WordPair> GetRandomPairFromCategoryAsync(string categoryName)
        {
            var pair = _wordPairService.GetRandomPairFromCategory(categoryName);
            return Task.FromResult(pair);
        }
    }
}
