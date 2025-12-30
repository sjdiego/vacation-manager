# Vacation Manager

Web application for managing team vacations.

## Tech Stack

- **Frontend**: Angular (TypeScript)
- **Backend**: .NET 8 Web API
- **Database**: SQLite (current)
- **Authentication**: Microsoft Entra ID (Azure AD)
- **Hosting**: Azure Static Web Apps (frontend) + Azure App Service (API)
- **Edge**: Azure Front Door (Standard)

## Project Structure

```
vacation-manager/
├── frontend/          # Angular Application
└── backend/           # .NET API
```

## Local development

### Prerequisites

- Node.js 20+
- .NET 8 SDK

### Frontend

```bash
cd frontend
npm install
npm start
```

### Backend

```bash
cd backend
dotnet restore
dotnet run --project VacationManager.Api
```

Default endpoints (dev):
- Frontend: http://localhost:4200
- API: https://localhost:<port>/ (see launchSettings)

## Production deployment (GitHub Actions)

This repo deploys via GitHub Actions:
- `.github/workflows/deploy-frontend.yml` → Azure Static Web Apps
- `.github/workflows/deploy-backend.yml` → Azure App Service

### Required GitHub Environment variables (production)

Configure these under **Settings → Environments → production → Variables**:

- `FRONTEND_API_URL` = `https://vacation-manager-api.azurewebsites.net/api`
- `FRONTEND_REDIRECT_URI` = `https://thankful-pond-0eb655f03.4.azurestaticapps.net//`
- `AZURE_RESOURCE_GROUP` = `vacation-manager-rg`
- `BACKEND_CORS_ALLOWED_ORIGINS` = `https://thankful-pond-0eb655f03.4.azurestaticapps.net`
  - Comma-separated if multiple, e.g. `https://a.com,https://b.com`

### Required GitHub Environment secrets (production)

Configure these under **Settings → Environments → production → Secrets**:

- `AZURE_STATIC_WEB_APPS_API_TOKEN`
- `AZURE_CREDENTIALS` (JSON from `az ad sp create-for-rbac --json-auth`)

> Note: you can’t view existing secret values in GitHub; if you don’t have them saved, regenerate them.

### CORS configuration

The API reads allowed origins from configuration:
- `Cors:AllowedOrigins` (App Service app settings: `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, ...)

In production, if no origins are configured, CORS will be effectively disabled (safe default).

## License

MIT
