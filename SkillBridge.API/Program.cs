using SkillBridge.DataAccess;
using SkillBridge.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Port=3306;Database=skillbridge_db;Uid=root;Pwd=;";

// Register DBHelper & Repositories
builder.Services.AddSingleton(new DBHelper(connectionString));
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<SkillRepository>();
builder.Services.AddScoped<RoadmapRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<CertificationRepository>();
builder.Services.AddScoped<InternshipRepository>();
builder.Services.AddScoped<ApplicationRepository>();
builder.Services.AddScoped<MentorshipRepository>();
builder.Services.AddScoped<ReadinessRepository>();
builder.Services.AddScoped<ReportRepository>();

// Configure CORS for Web app calls
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowWebClient");
app.UseAuthorization();
app.MapControllers();

app.Run();
