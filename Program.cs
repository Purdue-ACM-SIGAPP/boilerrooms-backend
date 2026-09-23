using dotenv.net;
using SimpleWebAppReact.Services;

// Load .env into environment variables before configuration is built,
// so entries like ConnectionStrings__DbConnection override appsettings.json
DotEnv.Load(new DotEnvOptions(probeForEnv: true, probeLevelsToSearch: 6));

const string CorsPolicy = "AllowAll";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// The Expo web and native clients call the API from other origins
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddHttpClient<BuildingOutlineService>();
builder.Services.AddHttpClient<GoogleMapsService>();
builder.Services.AddSingleton<PasswordHasherService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();

    // Set up Swagger UI to see documentation of all routes and controllers in this app
    app.MapOpenApi();
    app.UseSwaggerUI(options => {
        // Point Swagger to the OpenAPI spec
        options.SwaggerEndpoint("/openapi/v1.json", "My API v1");
    });
}

app.UseRouting();
app.UseCors(CorsPolicy);
app.MapControllers();

app.Run();
