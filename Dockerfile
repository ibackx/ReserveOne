# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files for restore (better layer caching)
COPY ReserveOne.sln ./
COPY ReserveOne.Domain/ReserveOne.Domain.csproj ReserveOne.Domain/
COPY ReserveOne.Application/ReserveOne.Application.csproj ReserveOne.Application/
COPY ReserveOne.Infrastructure/ReserveOne.Infrastructure.csproj ReserveOne.Infrastructure/
COPY ReserveOne.Api/ReserveOne.Api.csproj ReserveOne.Api/

RUN dotnet restore

# Copy all sources and publish API
COPY . .
RUN dotnet publish ReserveOne.Api/ReserveOne.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Render sets PORT. Default to 8080 if not provided.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

# Expand $PORT at runtime so Kestrel binds correctly on Render
CMD ["sh","-c","ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet ReserveOne.Api.dll"]
