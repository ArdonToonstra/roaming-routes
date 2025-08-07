using Microsoft.AspNetCore.Mvc;
using RoamingRoutes.Services;
using RoamingRoutes.Shared.Models.Games;

namespace RoamingRoutes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WordPairsController : ControllerBase
    {
        private readonly IWordPairService _wordPairService;
        private readonly ILogger<WordPairsController> _logger;

        public WordPairsController(IWordPairService wordPairService, ILogger<WordPairsController> logger)
        {
            _wordPairService = wordPairService;
            _logger = logger;
        }

        [HttpGet("categories")]
        public ActionResult<List<WordPairCategory>> GetCategories()
        {
            try
            {
                var categories = _wordPairService.GetCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving word pair categories");
                return StatusCode(500, "An error occurred while retrieving word pair categories");
            }
        }

        [HttpGet("categories/{categoryName}/random")]
        public ActionResult<WordPair> GetRandomPairFromCategory(string categoryName)
        {
            try
            {
                var pair = _wordPairService.GetRandomPairFromCategory(categoryName);
                return Ok(pair);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Category not found: {CategoryName}", categoryName);
                return NotFound($"Category '{categoryName}' not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving random word pair from category: {CategoryName}", categoryName);
                return StatusCode(500, "An error occurred while retrieving word pair");
            }
        }
    }
}
