FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PrivateECommerce.API/PrivateECommerce.API.csproj PrivateECommerce.API/
RUN dotnet restore PrivateECommerce.API/PrivateECommerce.API.csproj

COPY . .
WORKDIR /src/PrivateECommerce.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PrivateECommerce.API.dll"]