using Google.Cloud.Vision.V1;
using GoogleVisionSample.Models;
using Microsoft.AspNetCore.Mvc;

namespace GoogleVisionSample.Controllers
{
    public class VisionController : Controller
    {
        private readonly string _googleApiKeyPath = "orbital-ego-456115-n1-d8087bb9c03c.json";

        public VisionController()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _googleApiKeyPath);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return View("Index");
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var visionClient = ImageAnnotatorClient.Create();
                var image = Image.FromBytes(imageBytes);
                var response = await visionClient.DetectTextAsync(image);
                var textAnnotations = await visionClient.DetectTextAsync(image);
                var labelAnnotations = await visionClient.DetectLabelsAsync(image);
                var model = new VisionResultViewModel
                {
                    RecognizedText = string.Join(Environment.NewLine, textAnnotations.Select(t => t.Description)),
                    Labels = labelAnnotations
                        .Select(label => $"{label.Description} ({Math.Round(label.Score * 100)}%)")
                        .ToList()
                };

                return View("Result", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Analyze(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                var filePath = Path.GetTempFileName();

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var client = await ImageAnnotatorClient.CreateAsync();
                var googleImage = Image.FromFile(filePath);

                var textAnnotations = await client.DetectTextAsync(googleImage);
                var labelAnnotations = await client.DetectLabelsAsync(googleImage);

                var model = new VisionResultViewModel
                {
                    RecognizedText = string.Join(Environment.NewLine, textAnnotations.Select(t => t.Description)),
                    Labels = labelAnnotations
                        .Select(label => $"{label.Description} ({Math.Round(label.Score * 100)}%)")
                        .ToList()
                };

                return View("Result", model);
            }

            return RedirectToAction("Index");
        }
    }
}

