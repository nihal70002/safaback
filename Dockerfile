# ---------- BUILD STAGE ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore

# Publish ONLY the API project
RUN dotnet publish PrivateEcommerce.API/PrivateEcommerce.API.csproj -c Release -o /app/publish

# ---------- RUNTIME STAGE ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Railway uses 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Start API
ENTRYPOINT ["dotnet", "PrivateEcommerce.API.dll"]
