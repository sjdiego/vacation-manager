# Vacation Manager

Web application for managing team vacations with automatic integrations.

## Tech Stack

- **Frontend**: Angular 18+ with TypeScript
- **Backend**: .NET 8 Web API
- **Database**: Azure SQL Database
- **Authentication**: Microsoft Entra ID (Azure AD)
- **Hosting**: Azure App Service

## Project Structure

```
vacation-manager/
├── frontend/          # Angular Application
└── backend/           # .NET API
```

## Initial Setup

### Prerequisites

- Node.js 18+
- .NET 8 SDK
- Azure CLI
- Visual Studio Code or Visual Studio

### Frontend

```bash
cd frontend
npm install
ng serve
```

### Backend

```bash
cd backend
dotnet restore
dotnet run --project VacationManager.Api
```

## License

MIT
