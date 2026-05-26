# LinguaType

Monorepo for LinguaType (typing practice for multiple scripts/languages).

Services:

- `api` — ASP.NET Core (.NET 10) Web API
- `web` — React + Vite frontend

Quick start:

```bash
# run backend
cd api
dotnet restore
dotnet run --urls "http://localhost:5000"

# in another terminal, run frontend
cd web
npm install
npm run dev
```

The frontend proxies `/api` to `http://localhost:5000` during development.

Pinyin output now uses tone marks, for example `jīn tiān tiān qì hěn hǎo`.

Database setup:

- Set `api/appsettings.json` -> `ConnectionStrings:LinguaTypeDb` to your Supabase PostgreSQL connection string before starting the API.
- The API applies EF Core migrations on startup and seeds the first three typing samples automatically.
- Admin sample CRUD is available under `/api/admin/samples`.

Testing checklist:

- See [TESTING.md](TESTING.md)

Windows helpers:

```powershell
# start backend
.\run-api.ps1

# start frontend
.\run-web.ps1

# start both in separate windows
.\run-all.ps1
```
