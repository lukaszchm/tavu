using System.Net;
using Azure.Identity;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Tavu;
using Tavu.Exceptions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
}

builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddControllers(options =>
    {
        options.Filters.Add(new TavuExceptionFilter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureAuthntication(builder.Configuration);
builder.Services.ConfigureAzureStorage(builder.Configuration);
builder.Services.ConfigureExcerciseStore();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else 
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapFallbackToFile("/index.html");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Set the Swagger UI endpoint to /api/swagger
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        // Serve the Swagger UI at the specified URL. In this case, /api/swagger.
        c.RoutePrefix = "swagger";
    });
}

app.MapControllers();
app.MapGet("/login", (httpContext) => {
    var redirectPath = string.IsNullOrEmpty(httpContext.Request.QueryString.Value)
        ? string.Empty
        : httpContext.Request.QueryString.Value;
    redirectPath = "/" + redirectPath.TrimStart('?').Trim('/');
    httpContext.Response.Redirect(redirectPath);
    return Task.CompletedTask;
}).RequireAuthorization();

app.Run();
