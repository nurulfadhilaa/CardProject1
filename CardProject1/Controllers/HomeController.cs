using CardProject1.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting; // Add this using directive
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _env; // Add this field

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IWebHostEnvironment env) // Inject IWebHostEnvironment
    {
        _context = context;
        _logger = logger;
        _env = env;
    }

    public IActionResult Index()
    {
        return View();
    }

    // Metode untuk membuat data baru
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Tag tag)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving tag");
                return StatusCode(500, "Internal server error");
            }
        }

        // Log validation errors
        foreach (var modelState in ModelState.Values)
        {
            foreach (var error in modelState.Errors)
            {
                _logger.LogError(error.ErrorMessage);
            }
        }

        return BadRequest(ModelState); // Gagal
    }

    [HttpGet]
    public JsonResult List()
    {
        var tags = _context.Tags.Select(tag => new
        {
            Value = tag.Value,
            XCoordinate = tag.XCoordinate,
            YCoordinate = tag.YCoordinate
        }).ToList();

        return Json(tags);
    }

    // Metode untuk menghapus semua data
    [HttpPost]
    public async Task<IActionResult> ResetAll()
    {
        try
        {
            _context.Tags.RemoveRange(_context.Tags);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing tags");
            return StatusCode(500, "Internal server error");
        }
    }

    public IActionResult Generate()
    {
        // Construct paths
        string imagePath = Path.Combine(_env.WebRootPath, "uploads", "template.jpg");
        string fontPath = Path.Combine(_env.WebRootPath, "Font", "open-sans", "OpenSans-Regular.ttf");

        // Ensure the files exist
        if (!System.IO.File.Exists(imagePath) || !System.IO.File.Exists(fontPath))
        {
            return NotFound("Image or font file not found.");
        }

        // Load the image
        using (Image image = Image.Load(imagePath))
        {
            // List of text to draw
            var products = _context.Tags.ToList();

            // Set the font and brush for the text
            FontCollection fonts = new FontCollection();
            FontFamily fontFamily = fonts.Add(fontPath);
            Font font = fontFamily.CreateFont(24, FontStyle.Bold);

            // Draw each text item on the image
            foreach (var item in products)
            {
                image.Mutate(ctx => ctx.DrawText(item.Value, font, Color.Black, new PointF(item.XCoordinate, item.YCoordinate)));
            }

            // Save the modified image to memory stream and return as file
            using (var ms = new MemoryStream())
            {
                image.SaveAsJpeg(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms.ToArray(), "image/jpeg", "modified_image.jpg");
            }
        }
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile imageUpload)
    {
        if (imageUpload != null && imageUpload.Length > 0)
        {
            // Tentukan path untuk menyimpan file
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            string filePath = Path.Combine(uploadsFolder, "template.jpg");

            // Load the image using ImageSharp
            using (var stream = imageUpload.OpenReadStream())
            using (var image = Image.Load(stream))
            {
                // Resize the image (e.g., max width 800px, maintain aspect ratio)
                //int maxWidth = 800;
                //image.Mutate(ctx => ctx.Resize(new ResizeOptions
                //{
                //    Mode = ResizeMode.Max,
                //    Size = new Size(maxWidth, 0) // Setting height to 0 maintains aspect ratio
                //}));

                // Save the resized image to the specified path
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.SaveAsJpeg(fileStream);
                }
            }

            // Return URL gambar yang di-upload untuk ditampilkan
            string imageUrl = Url.Content("~/uploads/template.jpg");
            return Json(new { imageUrl });
        }

        return BadRequest("No image uploaded.");
    }


}