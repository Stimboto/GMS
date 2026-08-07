using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GMS.API.Middleware;
using GMS.Application.Interfaces;
using GMS.Infrastructure.Data;
using GMS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        builder => builder.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Configure Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAuthService, GMS.Application.Services.AuthService>();
builder.Services.AddScoped<IJwtTokenService, GMS.Infrastructure.Services.JwtTokenService>();
builder.Services.AddScoped<ICurrentUserService, GMS.Infrastructure.Services.CurrentUserService>();
builder.Services.AddScoped<IGrievanceRepository, GMS.Infrastructure.Repositories.GrievanceRepository>();
builder.Services.AddScoped<IGrievanceService, GMS.Application.Services.GrievanceService>();
builder.Services.AddScoped<IFileStorageService, GMS.Infrastructure.Services.FileStorageService>();
builder.Services.AddScoped<INotificationService, GMS.Infrastructure.Services.NotificationService>();
builder.Services.AddScoped<IDashboardService, GMS.Infrastructure.Services.DashboardService>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAIService, GMS.Infrastructure.Services.OllamaAIService>();
builder.Services.AddSingleton<IOllamaService, GMS.Infrastructure.Services.OllamaService>();
builder.Services.AddScoped<IRealTimeNotifier, GMS.API.Services.SignalRNotifier>();

builder.Services.AddScoped<IAuditLogRepository, GMS.Infrastructure.Repositories.AuditLogRepository>();
builder.Services.AddScoped<IAuditLogService, GMS.Application.Services.AuditLogService>();
builder.Services.AddScoped<IUserService, GMS.Application.Services.UserService>();
builder.Services.AddScoped<IDepartmentService, GMS.Application.Services.DepartmentService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "SuperSecretKeyForJwtAuthenticationWhichShouldBeAtLeast32Bytes");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CitizenPolicy", policy => policy.RequireRole("Citizen"));
    options.AddPolicy("OfficerPolicy", policy => policy.RequireRole("Officer"));
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await GMS.Infrastructure.Initialization.DbInitializer.InitializeAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GMS.API.Hubs.NotificationHub>("/hubs/notification");

app.Run();
