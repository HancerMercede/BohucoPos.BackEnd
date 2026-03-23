# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files
COPY ["NexusPOS.sln", "./"]
COPY ["NexusPOS.Domain/NexusPOS.Domain.csproj", "NexusPOS.Domain/"]
COPY ["NexusPOS.Application/NexusPOS.Application.csproj", "NexusPOS.Application/"]
COPY ["NexusPOS.Infrastructure/NexusPOS.Infrastructure.csproj", "NexusPOS.Infrastructure/"]
COPY ["NexusPOS.API/NexusPOS.API.csproj", "NexusPOS.API/"]

# Restore dependencies
RUN dotnet restore "NexusPOS.API/NexusPOS.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/NexusPOS.API"
RUN dotnet build "NexusPOS.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "NexusPOS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Install timezone data and SSL certificates, and EF Core tools
RUN apt-get update && apt-get install -y --no-install-recommends \
    tzdata \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/* \
    && dotnet tool install --global dotnet-ef

# Copy published files
COPY --from=publish /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV PATH="${PATH}:/root/.dotnet"

# Expose port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "NexusPOS.API.dll"]
