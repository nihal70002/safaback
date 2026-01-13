# ---------- BUILD STAGE ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project file first (for caching)
COPY PrivateEcommerce.API.sln .
COPY PrivateEcommerce.API/PrivateEcommerce.API.csproj PrivateEcommerce.API/

# Restore dependencies
RUN dotnet restore PrivateEcommerce.API/PrivateEcommerce.API.csproj

# Copy the remaining source code
COPY . .

# Publish the app
WORKDIR /src/PrivateEcommerce.API
RUN dotnet publish -c Release -o /app/publish

# ---------- RUNTIME STAGE ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PrivateEcommerce.API.dll"]
