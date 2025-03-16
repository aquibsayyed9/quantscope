# Base image for running the .NET application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
EXPOSE 9090

# Ensure logs are visible in stdout (important for Docker)
ENV DOTNET_USE_POLLING_FILE_WATCHER 1
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS false
#ENV ASPNETCORE_URLS http://0.0.0.0:8080
ENV ASPNETCORE_URLS http://0.0.0.0:9090
ENV DOTNET_RUNNING_IN_CONTAINER true

# Build stage: Compile the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies
COPY ["FixMessageAnalyzer.csproj", "."]
RUN dotnet restore "./FixMessageAnalyzer.csproj" # Ensure dependencies are restored

# Copy the full source code and build
COPY . .
WORKDIR "/src/."
RUN dotnet build "./FixMessageAnalyzer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage: Generate the final artifacts
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FixMessageAnalyzer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime stage
FROM base AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Ensure the log directory exists inside the container
RUN mkdir -p /app/logs

# Set Serilog to log to stdout and file
ENV SERILOG__WRITETO__0__NAME "Console"
ENV SERILOG__WRITETO__1__NAME "File"
ENV SERILOG__WRITETO__1__ARGS_PATH "/app/logs/app.log"

CMD ["dotnet", "FixMessageAnalyzer.dll"]
