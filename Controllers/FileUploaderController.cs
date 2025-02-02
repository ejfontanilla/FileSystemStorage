using FileSystemStorage.Interface;
using FileSystemStorage.Services;
using Microsoft.AspNetCore.Mvc;



namespace FileSystemStorage.Controllers
{
    [ApiController]
    [Route("api")]
    public class FileUploaderController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileUploaderController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Handles file uploads and stores in DynamoDb. It also calculates SHA-256.
        /// </summary>
        [HttpPost("uploadfile")]
        public async Task<IActionResult> UploadMultiPartFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            //string fileUrl = await _fileMultipartUploadService.UploadFileAsync(file);
            string fileUrl = await _fileService.UploadFileAsync(file);
            return Ok(new { Message = "File uploaded successfully", FileUrl = fileUrl });
        }

        /// <summary>
        /// Downloads a file that was uploaded recently.
        /// </summary>
        /// /// <param name="fileName">The file to download.</param>
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var fileBytes = await _fileService.GetFileAsync(fileName);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        /// <summary>
        /// Get the list of files uploaded.
        /// </summary>
        [HttpGet("files")]
        public async Task<IActionResult> ListFiles()
        {
            var files = await _fileService.ListFilesAsync();
            return Ok(files);
        }
    }
}
