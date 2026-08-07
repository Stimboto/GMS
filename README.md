<div align="center">
  <img width="1983" height="793" alt="ChatGPT Image Aug 8, 2026, 12_45_54 AM" src="https://github.com/user-attachments/assets/59caf91a-d4e1-4b1e-9cec-cfa453572486" />

# 🏛️ GMS

### **Report • Track • Resolve**

*An enterprise-grade platform designed to streamline issue reporting, tracking, and resolution between citizens and authorities.*

<p align="center">
<img src="https://img.shields.io/badge/Frontend-Angular_17-DD0031?style=for-the-badge&logo=angular&logoColor=white"/>
<img src="https://img.shields.io/badge/Backend-.NET_10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/Database-SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white"/>
<img src="https://img.shields.io/badge/AI-Ollama-000000?style=for-the-badge&logo=ollama&logoColor=white"/>
</p>

<p align="center">
<i>Transparent • Secure • AI-Driven</i>
</p>

</div>

---

## Architecture

- **Angular Frontend**: A modern, responsive Single Page Application (SPA) providing distinct dashboards for Citizens, Officers, and Admins.
- **ASP.NET Core API**: A highly performant REST API powering the business logic, security, and data access layer.
- **SQL Server**: A robust relational database handling structured data, user profiles, audit logs, and grievance records.
- **SignalR**: Real-time bidirectional communication powering instant notifications across the platform.
- **Ollama AI**: (Optional) Integrated AI service running on LLaMA for automated grievance sentiment analysis, priority detection, and smart assignments.
- **Docker**: The entire stack is containerized for seamless, platform-agnostic deployment.

## Features

- **Citizen**: Easy grievance submission, real-time status tracking, and satisfaction rating feedback.
- **Officer**: Dedicated workspace for handling assignments, uploading resolution proofs, and updating statuses.
- **Admin**: Comprehensive RBAC control, user management, and rich statistical dashboards.
- **AI**: Automated categorization and priority assignment of new grievances.
- **Notifications**: Real-time SignalR notifications and historical notification panel.
- **Analytics**: Workload distribution, status breakdowns, and monthly trend visualizations.
- **Attachments**: Secure image and document uploads for evidence and profiles.
- **Role Based Access**: Stricly enforced permissions across API and UI layers (Citizen, Officer, Admin).

## Technology Stack

- **Frontend**: Angular 17, Angular Material, Vanilla CSS (Glassmorphism), Chart.js
- **Backend**: ASP.NET Core 10.0, Entity Framework Core, SignalR, JWT Authentication
- **Database**: Microsoft SQL Server 2022
- **AI/ML**: Ollama (Local LLM Integration)
- **DevOps**: Docker, Docker Compose, Nginx


## Getting Started

### Prerequisites
- Docker Desktop installed on your machine.
- Git.

### 1. Clone repository
```bash
git clone https://github.com/yourusername/GMS.git
cd GMS
```

### 2. Configure Environment
Create the `.env` file from the provided example template:
```bash
cp .env.example .env
```
*(Optionally modify the passwords and keys in the `.env` file for production).*

### 3. Run Application
Run the entire stack using Docker Compose:
```bash
docker compose up --build
```

### Expected URLs after startup:
- **Frontend Application**: [http://localhost](http://localhost)
- **Backend API Base**: [http://localhost/api](http://localhost/api)
- **Swagger Documentation**: [http://localhost:8080/swagger](http://localhost:8080/swagger)

*(Note: The API is reverse-proxied through Nginx on port 80 under the `/api` and `/hubs` paths, but Swagger is exposed directly on port 8080 for development).*

## Docker

The `docker-compose.yml` orchestrates the following containers:
- `gms-frontend`: Nginx server hosting the compiled Angular static files and reverse-proxying API requests.
- `gms-api`: ASP.NET Core 10.0 runtime hosting the backend services. Automatically runs EF Core migrations on startup.
- `gms-sqlserver`: Official Microsoft SQL Server 2022 image. Data is persisted to a Docker volume.
- `gms-ollama` *(Optional)*: AI container for natural language processing tasks.

## AI Integration

To enable the AI capabilities, simply uncomment the `gms-ollama` service in `docker-compose.yml` before running `docker compose up`. The API is pre-configured to communicate with it over the internal Docker network.

## Folder Structure

```
GMS/
├── ClientApp/                # Angular Frontend Application
│   ├── src/                  # Source Code (Components, Services, Assets)
│   ├── Dockerfile            # Multi-stage build for Frontend + Nginx
│   └── nginx.conf            # Nginx Reverse Proxy Configuration
├── GMS.API/                  # ASP.NET Core API Presentation Layer
├── GMS.Application/          # Business Logic, Interfaces, CQRS
├── GMS.Domain/               # Enterprise Domain Entities
├── GMS.Infrastructure/       # EF Core Data Access, Repositories, AI Integration
├── Dockerfile                # Multi-stage build for ASP.NET Backend
├── docker-compose.yml        # Multi-container Orchestration
└── .env.example              # Example Environment Variables
```

## Deployment

Any developer can quickly spin up this project. By cloning the repository and running `docker compose up --build`, the entire application—including the database schema and admin seeding—will automatically build and configure itself. No local installation of .NET SDKs, Node.js, or SQL Server is required.
