using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoamingRoutes.Data;
using RoamingRoutes.Shared.Models;

[Route("api/[controller]")]
[ApiController]
public class CityGuidesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public CityGuidesController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: api/cityguides
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CityGuide>>> GetCityGuides()
    {
        var guides = await _context.CityGuides.ToListAsync();
        foreach (var guide in guides)
        {
            FindAndAssignHeaderPhoto(guide);
        }
        return guides;
    }

    // GET: api/cityguides/netherlands-amsterdam
    [HttpGet("{UrlKey}")]
    public async Task<ActionResult<CityGuide>> GetCityGuide(string UrlKey)
    {
        var guide = await _context
            .CityGuides.Include(g => g.Sections)
            .ThenInclude(s => s.Highlights)
            .FirstOrDefaultAsync(g => g.UrlKey.ToLower() == UrlKey.ToLower());

        if (guide == null)
            return NotFound();

        FindAndAssignHeaderPhoto(guide);
        // In de toekomst kun je hier ook foto's voor highlights toewijzen.
        return guide;
    }

    private void FindAndAssignHeaderPhoto(CityGuide guide)
    {
        var guideImageDirectory = Path.Combine(
            _env.WebRootPath,
            "images",
            "cities",
            guide.UrlKey ?? ""
        );
        if (Directory.Exists(guideImageDirectory))
        {
            var headerFile = Directory.GetFiles(guideImageDirectory, "header-1.*").FirstOrDefault();
            if (headerFile != null)
            {
                guide.HeaderImageUrl =
                    $"/images/cities/{guide.UrlKey}/{Path.GetFileName(headerFile)}";
            }
        }
    }
}
