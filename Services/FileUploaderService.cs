using Amazon.S3;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Security.Cryptography;
using FileSystemStorage.Interface;
using FileSystemStorage.Model;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Net.Security;
using Amazon.S3.Model;

namespace FileSystemStorage.Services
{
    public class FileUploaderService : IFileService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly DynamoDBService _dynamoDbService;
        private readonly AWSSettingsModel _awsSettings;

        public FileUploaderService(IAmazonDynamoDB dynamoDbClient, IConfiguration configuration, 
            IOptions<AWSSettingsModel> awsSettings, DynamoDBService dynamoDbService)
        {
            _dynamoDbService = dynamoDbService;
            _awsSettings = awsSettings.Value;
            _s3Client = new AmazonS3Client(
                _awsSettings.AccessKey,
                _awsSettings.SecretKey,
                new AmazonS3Config
                {
                    ServiceURL = _awsSettings.ServiceURL,
                    ForcePathStyle = true
                });
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            using var fileStream = file.OpenReadStream();
            using var shaStream = new SHA256Stream(fileStream);

            string fileUrl = await Upload(shaStream, file.FileName);
            shaStream.Close();
            await _dynamoDbService.StoreFileHashAsync(file.FileName, shaStream.Hash);

            return fileUrl;
        }

        private async Task<string> Upload(Stream fileStream, string fileName)
        {
            using var sha256 = SHA256.Create();
            using var hashingStream = new CryptoStream(Stream.Null, sha256, CryptoStreamMode.Write); // Hashing stream (dummy output)

            using var teeStream = new TeeStream(fileStream, hashingStream); // Copying to hash while reading

            var initiateRequest = new InitiateMultipartUploadRequest { BucketName = _awsSettings.BucketName, Key = fileName };
            var initResponse = await _s3Client.InitiateMultipartUploadAsync(initiateRequest);

            var partETags = new List<PartETag>();
            const int partSize = 5 * 1024 * 1024; // 5MB chunk size
            var buffer = new byte[partSize];
            int bytesRead;
            int partNumber = 1;

            while ((bytesRead = await teeStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                using var memoryStream = new MemoryStream();
                await memoryStream.WriteAsync(buffer, 0, bytesRead);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var uploadRequest = new UploadPartRequest
                {
                    BucketName = _awsSettings.BucketName,
                    Key = fileName,
                    UploadId = initResponse.UploadId,
                    PartNumber = partNumber++,
                    InputStream = memoryStream
                };

                var uploadResponse = await _s3Client.UploadPartAsync(uploadRequest);
                partETags.Add(new PartETag(uploadResponse.PartNumber, uploadResponse.ETag));
            }

            var completeRequest = new CompleteMultipartUploadRequest
            {
                BucketName = _awsSettings.BucketName,
                Key = fileName,
                UploadId = initResponse.UploadId,
                PartETags = partETags
            };

            _s3Client.CompleteMultipartUploadAsync(completeRequest);

            // Compute final SHA-256 hash after full read
            hashingStream.FlushFinalBlock();

            string hashHex = BitConverter.ToString(sha256.Hash).Replace("-", "").ToLower();

            //Console.WriteLine($"SHA-256 Hash: {hashHex}");

            return $"https://{_awsSettings.BucketName}.s3.amazonaws.com/{fileName}";
        }

        public async Task<byte[]> GetFileAsync(string fileName)
        {
            var response = await _s3Client.GetObjectAsync(_awsSettings.BucketName, fileName);
            using var stream = response.ResponseStream;
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }

        public async Task<IEnumerable<string>> ListFilesAsync()
        {
            var response = await _s3Client.ListObjectsAsync(_awsSettings.BucketName);
            return response.S3Objects.Select(o => o.Key);
        }

        private static string ComputeSha256Hash(Stream input)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(input);
            return Convert.ToHexString(hashBytes);
        }
    }
}
