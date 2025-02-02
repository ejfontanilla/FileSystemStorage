using Amazon.S3;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using FileSystemStorage.Interface;
using FileSystemStorage.Services;
using FileSystemStorage.Model;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Configure AWS services
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.Configure<AWSSettingsModel>(builder.Configuration.GetSection("AWS"));
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "File Storage API",
        Version = "v1",
        Description = "API for uploading, retrieving, and listing files with AWS S3 and DynamoDB integration."
    });

    // Enable XML comments (optional)
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Configure custom services
builder.Services.AddScoped<IFileService, FileUploaderService>();
builder.Services.AddScoped<DynamoDBService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Storage API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
