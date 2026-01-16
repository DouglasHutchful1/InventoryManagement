# ---------- BUILD STAGE ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore InventoryManagementSystem.csproj

# Publish the app
RUN dotnet publish InventoryManagementSystem.csproj \
    -c Release \
    -o /app/publish

# ---------- RUNTIME STAGE ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
EXPOSE 8080

ENTRYPOINT ["dotnet", "InventoryManagementSystem.dll"]
