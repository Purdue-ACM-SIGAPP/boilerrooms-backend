using dotenv.net;
using Microsoft.OpenApi.Models;
using SimpleWebAppReact.Services;

// Load .env into environment variables before configuration is built,
// so entries like ConnectionStrings__DbConnection override appsettings.json
DotEnv.Load(new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 6));

const string CorsPolicy = "AllowAll";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Boiler Rooms API", Version = "v1" }));

// The Expo web and native clients call the API from other origins
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddHttpClient<BuildingOutlineService>();
builder.Services.AddHttpClient<GoogleMapsService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseCors(CorsPolicy);
app.MapControllers();

app.Run();
