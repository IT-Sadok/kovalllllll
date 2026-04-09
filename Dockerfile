# UI build stage
FROM node:20-alpine AS ui-build
WORKDIR /src/DroneBuilder.UI

COPY ["DroneBuilder.UI/package.json", "DroneBuilder.UI/package-lock.json", "./"]
RUN npm install --no-audit --no-fund

COPY ["DroneBuilder.UI/", "./"]

# Optional override for split deployments. In single-container mode /api is correct.
ARG VITE_API_BASE_URL=/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL

RUN npm run build

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and all project files
COPY ["DroneBuilder/DroneBuilder.sln", "DroneBuilder/"]
COPY ["DroneBuilder/DroneBuilder.API/DroneBuilder.API.csproj", "DroneBuilder/DroneBuilder.API/"]
COPY ["DroneBuilder/DroneBuilder.Application/DroneBuilder.Application.csproj", "DroneBuilder/DroneBuilder.Application/"]
COPY ["DroneBuilder/DroneBuilder.Infrastructure/DroneBuilder.Infrastructure.csproj", "DroneBuilder/DroneBuilder.Infrastructure/"]
COPY ["DroneBuilder/DroneBuilder.Domain/DroneBuilder.Domain.csproj", "DroneBuilder/DroneBuilder.Domain/"]
COPY ["DroneBuilder/DroneBuilder.Application.Tests/DroneBuilder.Application.Tests.csproj", "DroneBuilder/DroneBuilder.Application.Tests/"]

# Restore dependencies
RUN dotnet restore "DroneBuilder/DroneBuilder.sln"

# Copy all source code
COPY . .

# Copy built frontend into API static files folder
COPY --from=ui-build /src/DroneBuilder.UI/dist /src/DroneBuilder/DroneBuilder.API/wwwroot

# Build
WORKDIR "/src/DroneBuilder/DroneBuilder.API"
RUN dotnet publish "DroneBuilder.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DroneBuilder.API.dll"]
