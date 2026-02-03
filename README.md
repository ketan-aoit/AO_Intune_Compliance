# Intune Compliance Portal

A web portal for A&O IT Group to monitor device compliance against Cyber Essentials requirements, integrated with Microsoft Intune via Graph API.

## Features

- **Dashboard** - Real-time compliance overview with statistics, devices at risk, and recent alerts
- **Device Management** - Searchable, filterable, and sortable device list with detailed compliance information
- **Compliance Rules** - Configure rules for OS versions, browsers, and security software
- **Alerts** - Email notifications for devices approaching end-of-support dates
- **Role-Based Access** - Admin, Manager, and Viewer roles with appropriate permissions

## Technology Stack

### Backend
- .NET 8 with Clean Architecture
- CQRS pattern using MediatR
- Entity Framework Core with Azure SQL Database
- Hangfire for background job processing
- Azure AD authentication with JWT Bearer tokens

### Frontend
- React 18 with TypeScript
- Fluent UI v9 components
- TanStack Query for server state management
- MSAL React for authentication
- Vite for build tooling

### Infrastructure
- Azure App Service (API)
- Azure Static Web Apps (Frontend)
- Azure SQL Database
- Azure Key Vault (secrets)
- Application Insights (monitoring)

## Project Structure

```
/AOIntuneAlerts
├── src/
│   ├── AOIntuneAlerts.Domain/           # Domain entities, value objects, enums
│   ├── AOIntuneAlerts.Application/      # CQRS handlers, interfaces, DTOs
│   ├── AOIntuneAlerts.Infrastructure/   # EF Core, Graph API services
│   ├── AOIntuneAlerts.BackgroundJobs/   # Hangfire scheduled jobs
│   ├── AOIntuneAlerts.WebApi/           # REST API controllers
│   └── AOIntuneAlerts.Web/              # React SPA frontend
├── tests/                               # Unit and integration tests
├── infra/                               # Bicep infrastructure templates
└── .github/workflows/                   # CI/CD pipelines
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Azure subscription
- Azure AD tenant with Intune

### Backend Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/AO-IT-Services/AO_InTune_Compliance_Portal.git
   cd AO_InTune_Compliance_Portal
   ```

2. Configure app settings:
   ```bash
   cp src/AOIntuneAlerts.WebApi/appsettings.Development.json.example src/AOIntuneAlerts.WebApi/appsettings.Development.json
   ```

   Update with your Azure AD and database settings.

3. Run database migrations:
   ```bash
   dotnet ef database update --project src/AOIntuneAlerts.Infrastructure
   ```

4. Start the API:
   ```bash
   dotnet run --project src/AOIntuneAlerts.WebApi
   ```

### Frontend Setup

1. Navigate to the web project:
   ```bash
   cd src/AOIntuneAlerts.Web
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Configure environment:
   ```bash
   cp .env.example .env.local
   ```

   Update with your API URL and Azure AD settings.

4. Start the development server:
   ```bash
   npm run dev
   ```

### Running Tests

**Backend tests:**
```bash
dotnet test
```

**Frontend E2E tests:**
```bash
cd src/AOIntuneAlerts.Web
npm run test:e2e
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/dashboard` | Dashboard statistics |
| GET | `/api/v1/devices` | List devices (paginated, filterable, sortable) |
| GET | `/api/v1/devices/{id}` | Device details |
| POST | `/api/v1/devices/sync` | Trigger Intune sync (Admin) |
| POST | `/api/v1/devices/{id}/evaluate` | Evaluate device compliance (Admin) |
| GET | `/api/v1/compliance-rules` | List compliance rules |
| GET | `/api/v1/alerts/history` | Alert history |
| GET | `/api/v1/alerts/recipients` | Alert recipients |

## Compliance States

| State | Description | Badge Color |
|-------|-------------|-------------|
| Compliant | Device meets all requirements | Green |
| NonCompliant | Device fails one or more requirements | Red |
| InGracePeriod | Device has time to become compliant | Orange |
| ApproachingEndOfSupport | OS/software nearing end of support | Orange |
| ConfigManager | Managed by Configuration Manager | Blue |
| Conflict | Conflicting compliance policies | Red |
| Error | Error evaluating compliance | Red |
| Unknown | Compliance state not determined | Grey |

## Background Jobs

| Job | Schedule | Description |
|-----|----------|-------------|
| DeviceSyncJob | Every 4 hours | Syncs devices from Intune |
| ComplianceEvaluationJob | Daily at 2 AM | Evaluates all devices against rules |
| AlertProcessingJob | Daily at 8 AM | Sends email alerts for at-risk devices |

## Azure AD Configuration

### Portal Authentication App
- Platform: Single-page application (SPA)
- Redirect URI: `https://your-app.azurestaticapps.net`
- Delegated permissions: `User.Read`, `openid`, `profile`

### Graph API Service App
- Application permissions (require admin consent):
  - `DeviceManagementManagedDevices.Read.All`
  - `DeviceManagementConfiguration.Read.All`
  - `Mail.Send`

## Deployment

The application uses GitHub Actions for CI/CD:

- **Backend**: Deploys to Azure App Service
- **Frontend**: Deploys to Azure Static Web Apps
- **Infrastructure**: Bicep templates for Azure resources

## License

Proprietary - A&O IT Group

## Support

For issues or questions, contact the A&O IT Group development team.
