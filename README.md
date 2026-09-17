# Boiler Rooms Backend

ASP.NET Core 6.0 Web API and MongoDB project

### Configure

Copy `.env.example` to `.env` and fill in the keys you need. Values in `.env` override `appsettings.json`:

- `ConnectionStrings__DbConnection` – MongoDB connection string (defaults to `mongodb://localhost:27017` in Development)
- `GoogleMaps__ApiKey` – geocoding and distance endpoints

The API is open: there is no JWT validation yet. `POST api/User` creates an account from a
username and password, and its id is looked up by hand with `GET api/User?username=<name>`.

### Local database

Start MongoDB in Docker. On first start it is seeded from `seed/seed.js` with the buildings outlined in the frontend map plus sample reviews and events:

```
docker compose up -d
```

To reseed, remove the volume: `docker compose down -v && docker compose up -d`.

### Install required dependencies:
```
dotnet restore
```

Run in localhost (http://localhost:5128, Swagger at `/swagger`):

```
dotnet run
```
