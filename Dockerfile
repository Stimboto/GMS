# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching for NuGet restore
COPY ["GMS.slnx", "./"]
COPY ["GMS.API/GMS.API.csproj", "GMS.API/"]
COPY ["GMS.Application/GMS.Application.csproj", "GMS.Application/"]
COPY ["GMS.Domain/GMS.Domain.csproj", "GMS.Domain/"]
COPY ["GMS.Infrastructure/GMS.Infrastructure.csproj", "GMS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "GMS.API/GMS.API.csproj"

# Copy the remaining source code
COPY . .

# Build and publish the application
WORKDIR "/src/GMS.API"
RUN dotnet publish "GMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Expose port 8080 (the default port for .NET 8+ inside containers)
EXPOSE 8080

# Configure healthcheck
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/swagger/index.html || exit 1

ENTRYPOINT ["dotnet", "GMS.API.dll"]
