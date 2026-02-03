# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and all project files
COPY ["DroneBuilder/DroneBuilder.sln", "DroneBuilder/"]
COPY ["DroneBuilder/DroneBuilder.API/DroneBuilder.API.csproj", "DroneBuilder/DroneBuilder.API/"]
COPY ["DroneBuilder/DroneBuilder.Application/DroneBuilder.Application.csproj", "DroneBuilder/DroneBuilder.Application/"]
COPY ["DroneBuilder/DroneBuilder.Infrastructure/DroneBuilder.Infrastructure.csproj", "DroneBuilder/DroneBuilder.Infrastructure/"]
COPY ["DroneBuilder/DroneBuilder.Domain/DroneBuilder.Domain.csproj", "DroneBuilder/DroneBuilder.Domain/"]

# Restore dependencies
RUN dotnet restore "DroneBuilder/DroneBuilder.sln"

# Copy all source code
COPY . .

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
