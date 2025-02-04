using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using FileSystemStorage.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;

namespace FileSystemStorage.Services
{
    public class DynamoDBService
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly Table _table;
        private readonly AWSSettingsModel _awsSettings;

        public DynamoDBService(IConfiguration configuration, IOptions<AWSSettingsModel> awsSettings)
        {
            _awsSettings = awsSettings.Value;
            var dynamoDbConfig = new AmazonDynamoDBConfig
            {
                ServiceURL = _awsSettings.ServiceURL,  // Use LocalStack endpoint
            };
            _dynamoDbClient = new AmazonDynamoDBClient(_awsSettings.AccessKey, _awsSettings.SecretKey, dynamoDbConfig);
            _table = Table.LoadTable(_dynamoDbClient, _awsSettings.DynamoTableName);
        }

        public async Task StoreFileHashAsync(string fileName, byte[] hash)
        {
            var doc = new Document
            {
                ["Filename"] = fileName,
                ["hash"] = BitConverter.ToString(hash).Replace("-", "").ToLower(),
                ["UploadedAt"] = DateTime.UtcNow.ToString("o")
            };

            await _table.PutItemAsync(doc);
        }
    }
}
