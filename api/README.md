# LinguaType API

Minimal ASP.NET Core (.NET 10) Web API skeleton for LinguaType.

Endpoints:

- GET /api/samples -> list of sample Chinese texts
- GET /api/pinyin?text=... -> returns pinyin for given text (using TinyPinyin.Net)
- GET /api/admin/samples -> list samples from PostgreSQL
- POST /api/admin/samples -> create a sample
- PUT /api/admin/samples/{id} -> update a sample
- DELETE /api/admin/samples/{id} -> delete a sample

Run:

```bash
cd api
dotnet restore
dotnet run --urls "http://localhost:5000"
```

Database:

- Put your Supabase PostgreSQL connection string in `api/appsettings.json` under `ConnectionStrings:LinguaTypeDb`.
- The API applies migrations on startup.
- The first three typing samples are seeded automatically.
