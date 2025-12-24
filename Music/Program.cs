using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Music.Data;
using Music.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================
// KESTREL
// ============================
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // Flutter
    options.ListenAnyIP(7143); // Swagger
});

// ============================
// SERVICES
// ============================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ChartSnapshotService>();
builder.Services.AddHostedService<ChartSnapshotWorker>();



// ============================
// DATABASE
// ============================
builder.Services.AddDbContext<MusicDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))
    )
);

// ============================
// CORS
// ============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ============================
// JWT AUTH
// ============================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var jwt = builder.Configuration.GetSection("Jwt");
    var key = Encoding.UTF8.GetBytes(jwt["Key"]);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ============================
// MIDDLEWARE
// ============================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ============================
// STATIC FILES + STREAMING
// ============================
void UseStaticWithRange(string folder, string requestPath)
{
    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);
    Directory.CreateDirectory(path);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(path),
        RequestPath = requestPath,
        ServeUnknownFileTypes = true,
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Accept-Ranges", "bytes");
        }
    });
}

// 🔊 Audio (MP3 streaming)
UseStaticWithRange("audio", "/audio");

// 🖼 Covers
UseStaticWithRange("covers", "/covers");

// 👤 Artist avatar
UseStaticWithRange("avatar_artist", "/avatar_artist");

// 👤 User avatar
UseStaticWithRange("uploads/avatars", "/uploads/avatars");

app.MapControllers();
app.Run();
