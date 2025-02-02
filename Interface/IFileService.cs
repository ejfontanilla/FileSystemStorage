namespace FileSystemStorage.Interface
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file);
        Task<byte[]> GetFileAsync(string fileName);
        Task<IEnumerable<string>> ListFilesAsync();
    }
}
