# ---------- BUILD STAGE ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution and project files
COPY PrivateEcommerce.API.sln .
COPY PrivateEcommerce.API/PrivateEcommerce.API.csproj PrivateEcommerce.API/

# restore
RUN dotnet restore PrivateEcommerce.API/PrivateEcommerce.API.csproj

# copy everything else
COPY . .

# publish
WORKDIR /src/PrivateEcommerce.API
RUN dotnet publish -c Release -o /app/publish

# ---------- RUNTIME STAGE ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PrivateEcommerce.API.dll"]
