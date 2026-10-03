using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DocQuery.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("Supabase:Url is not configured.");
var supabaseJwtAudience = builder.Configuration["Supabase:JwtAudience"] ?? "authenticated";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Supabase publishes signing keys through its OIDC discovery document/JWKS endpoint.
        // This validates access tokens without handling or storing a Supabase secret key.
        options.Authority = $"{supabaseUrl.TrimEnd('/')}/auth/v1";
        options.Audience = supabaseJwtAudience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"{supabaseUrl.TrimEnd('/')}/auth/v1",
            ValidateAudience = true,
            ValidAudience = supabaseJwtAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "sub"
        };
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:Origins").Get<string[]>()
            ?? new[] { "http://localhost:4200" };

        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
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

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<QdrantService>().CreateCollectionAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}

app.UseCors("Angular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
