using DocQuery.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient<OllamaService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434/");
    });
    builder.Services.AddScoped<ILlmService>(sp => sp.GetRequiredService<OllamaService>());
}
else
{
    builder.Services.AddHttpClient<GeminiService>(client =>
    {
        client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    });
    builder.Services.AddScoped<ILlmService>(sp => sp.GetRequiredService<GeminiService>());
}

builder.Services.AddSingleton<QdrantService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<TextChunker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Angular");

app.UseAuthorization();

app.MapControllers();

app.Run();
