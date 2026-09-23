# UI build stage
FROM node:20-alpine AS ui-build
WORKDIR /src/client

COPY ["client/package.json", "client/package-lock.json", "./"]
RUN npm ci --no-audit --no-fund

COPY ["client/", "./"]

# Optional override for split deployments. In single-container mode /api is correct.
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src/server

# Copy solution and all project files
COPY ["server/global.json", "server/DroneBuilder.sln", "./"]
COPY ["server/src/DroneBuilder.API/DroneBuilder.API.csproj", "src/DroneBuilder.API/"]
COPY ["server/src/DroneBuilder.Application/DroneBuilder.Application.csproj", "src/DroneBuilder.Application/"]
COPY ["server/src/DroneBuilder.Infrastructure/DroneBuilder.Infrastructure.csproj", "src/DroneBuilder.Infrastructure/"]
COPY ["server/src/DroneBuilder.Domain/DroneBuilder.Domain.csproj", "src/DroneBuilder.Domain/"]
COPY ["server/tests/DroneBuilder.Application.Tests/DroneBuilder.Application.Tests.csproj", "tests/DroneBuilder.Application.Tests/"]

# Restore dependencies
RUN dotnet restore "DroneBuilder.sln"

# Copy all source code
COPY ["server/", "./"]

# Copy built frontend into API static files folder
COPY --from=ui-build /src/client/dist /src/server/src/DroneBuilder.API/wwwroot

# Build
WORKDIR "/src/server/src/DroneBuilder.API"
RUN dotnet publish "DroneBuilder.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .

# The image ships a non-root account; both exposed ports are above 1024, so nothing needs root.
USER $APP_UID

ENTRYPOINT ["dotnet", "DroneBuilder.API.dll"]
