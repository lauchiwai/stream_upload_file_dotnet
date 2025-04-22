using Microsoft.OpenApi.Models;
using WebApplication1.Data.Options;
using WebApplication1.Helpers;
using WebApplication1.Services.Implementations;
using WebApplication1.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<FileUploadOptions>(options =>
{
    options.MultipartBoundaryLengthLimit = 128;
    options.ValueCountLimit = 1024;
    options.MultipartBodyLengthLimit = 1000 * 1024 * 1024;
    options.AllowedExtensions = new string[] { ".jpg", ".png", ".pdf", ".mp3", ".m4a" };
});

builder.Services.AddScoped<IMultipartRequestHelper, MultipartRequestHelper>();
builder.Services.AddScoped<IFileStreamingHelper, FileStreamingHelper>();
builder.Services.AddScoped<ILocalUploadService, LocalUploadService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "File Upload API", Version = "v1" });

    //  multipart/form-data
    c.MapType<HttpRequest>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["file"] = new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            }
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("OpenCorsPolicy", builder => builder
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 使用 CORS
app.UseCors("OpenCorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
