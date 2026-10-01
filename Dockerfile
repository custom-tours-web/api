# BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and configuration
COPY ["api.sln", "./"]
COPY ["api/api.csproj", "api/"]
COPY ["global.json", "nuget.config", "Directory.Packages.props", "Directory.Build.props", "Directory.Build.targets", "./"]

# Restore dependencies
RUN dotnet restore "api/api.csproj"

# Copy the remaining application code (including the SQL script)
COPY . .
WORKDIR "/src/api"

# Build and publish
RUN dotnet publish "api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# RUNTIME STAGE
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install SQLite to run the raw SQL initialization script
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=build /app/publish .
# Copy the database initialization script
COPY Schemas/Tables/BookingRequests.sql .

# Expose the HTTP port defined in launchSettings.json
EXPOSE 5166

# Create a startup script that initializes the database, then starts the API
RUN echo '#!/bin/bash\n\
mkdir -p /app/data\n\
sqlite3 /app/data/tourism.db < BookingRequests.sql\n\
exec dotnet api.dll' > start.sh && chmod +x start.sh

ENTRYPOINT ["./start.sh"]
