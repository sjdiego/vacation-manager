# Monorepo Setup Guide

## Project Structure

```
vacation-manager/
├── frontend/                 # Angular 19 application
│   ├── src/
│   ├── package.json
│   ├── angular.json
│   └── .env.*                # Environment-specific configs
├── backend/                  # .NET 8 API
│   ├── VacationManager.Api/
│   ├── VacationManager.Core/
│   ├── VacationManager.Data/
│   ├── VacationManager.Tests/
│   ├── appsettings.json
│   └── appsettings.*.json    # Environment-specific configs
├── package.json              # Root monorepo config
└── .editorconfig             # Shared editor config
```

## Available Commands

### Setup
```bash
npm run setup              # Install all dependencies
```

### Development
```bash
npm run frontend:serve    # Start Angular dev server (http://localhost:4200)
npm run backend:run       # Start .NET API (https://localhost:54194)
npm run dev              # Run both frontend and backend
```

### Building
```bash
npm run frontend:build          # Build frontend (dev)
npm run frontend:build:prod     # Build frontend (production)
npm run backend:build          # Build backend
npm run build                  # Build both (production)
```

### Testing
```bash
npm run frontend:test    # Run Angular tests
npm run backend:test     # Run .NET tests
npm run test            # Run all tests
```

### Linting
```bash
npm run frontend:lint    # Lint Angular code
npm run backend:lint     # Format .NET code
npm run lint            # Lint everything
```

### Cleanup
```bash
npm run clean           # Clean build artifacts
```

## Environment Configuration

### Frontend (.env files)
- `.env.development` - Local development config
- `.env.production` - Production config

Update these with your actual values:
- `API_URL` - Backend API endpoint
- `MSAL_CLIENT_ID` - Azure AD app ID
- `MSAL_AUTHORITY` - Azure AD tenant
- `MSAL_REDIRECT_URI` - Redirect URI for auth

### Backend (appsettings files)
- `appsettings.Development.json` - Development config
- `appsettings.Production.json` - Production config

Key settings:
- `AllowedOrigins` - CORS whitelist
- `AzureAd` - Auth configuration
- `RateLimiting` - API rate limiting

## Benefits of This Monorepo Setup

1. **Single repository** - Frontend and backend in one place
2. **Unified scripts** - Run commands from root
3. **Shared configuration** - EditorConfig for consistent formatting
4. **Environment management** - Separate configs for dev/prod
5. **Simplified CI/CD** - Build and deploy from single trigger
6. **Easier onboarding** - Single repo to clone

## Deployment Scenarios

### Local Development
```bash
npm run dev
```

### Azure Deployment
1. Update `appsettings.Production.json` with Azure values
2. Deploy frontend and backend to Azure App Service
3. Configure Azure AD app registration
4. Set environment variables in Azure

### GitHub Actions CI/CD
Pipelines will install from `package.json` and `.csproj` files automatically.
