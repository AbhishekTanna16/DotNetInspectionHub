# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files
COPY ../.. .

# Restore dependencies
RUN dotnet restore DotNetInspectionHub/ShopInspector.Web/ShopInspector.Web.csproj

# Publish in Release mode
RUN dotnet publish DotNetInspectionHub/ShopInspector.Web/ShopInspector.Web.csproj -c Release -o /app/out


# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Set URL for Render (Render injects PORT environment variable)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copy published files from build stage
COPY --from=build /app/out .

# Start the app
ENTRYPOINT ["dotnet", "ShopInspector.Web.dll"]
