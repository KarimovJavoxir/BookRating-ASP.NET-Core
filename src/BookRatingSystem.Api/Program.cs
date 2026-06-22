using BookRatingSystem.Application;
using BookRatingSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

const string ViteFrontendCorsPolicy = "ViteFrontend";

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        ViteFrontendCorsPolicy,
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();

app.UseCors(ViteFrontendCorsPolicy);

app.MapControllers();

app.Run();
