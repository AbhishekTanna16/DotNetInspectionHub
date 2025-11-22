# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy everything from the web project folder
COPY . .

# Restore dependencies for this project
RUN dotnet restore ShopInspector.Web.csproj

# Publish in Release mode
RUN dotnet publish ShopInspector.Web.csproj -c Release -o out


# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Render will inject PORT, we bind to it
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copy published files
COPY --from=build /app/out .

# Start the app
ENTRYPOINT ["dotnet", "ShopInspector.Web.dll"]
