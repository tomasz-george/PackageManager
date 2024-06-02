using Microsoft.AspNetCore.Mvc;
using PackageManager.Application.Interfaces;

namespace PackageManager.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PackageController(ILogger<PackageController> logger, IPackageValidationService packageValidationService) : ControllerBase
    {
        private readonly IPackageValidationService _packageValidationService = packageValidationService;
        private readonly ILogger<PackageController> _logger = logger;

        [HttpPost("validate-installation")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            if (!file.ContentType.Equals("text/plain"))
            {
                _logger.LogWarning("Unsupported media type.");
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, "Unsupported media type.");
            }

            string[] fileContent;
            string inputFileName = file.FileName;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var content = await reader.ReadToEndAsync();
                fileContent = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            _logger.LogInformation($"Validating installation file with content: {string.Join(", ", fileContent)}");

            var isValid = _packageValidationService.IsInstallationValid(fileContent);
            _logger.LogInformation($"Validation completed with result: {isValid}");

            var responseContent = _packageValidationService.ProduceResponseContent(isValid);

            var byteArray = System.Text.Encoding.UTF8.GetBytes(responseContent);
            var stream = new MemoryStream(byteArray);

            string outputFileName = inputFileName.Contains("input") ? inputFileName.Replace("input", "output") : $"{inputFileName}_output";

            return File(stream, ContentTypes.TextFile, outputFileName);
        }
    }
}
