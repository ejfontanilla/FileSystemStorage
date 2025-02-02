﻿namespace FileSystemStorage.Model
{
    public class AWSSettingsModel
    {
        public string ServiceURL { get; set; }
        public string BucketName { get; set; }
        public string DynamoTableName { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
    }
}
