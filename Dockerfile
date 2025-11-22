# ===== BUILD STAGE =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy entire repo into the container
COPY . .

# Restore dependencies for the Web project (and its referenced projects)
RUN dotnet restore ShopInspector.Web/ShopInspector.Web.csproj

# Publish in Release mode
RUN dotnet publish ShopInspector.Web/ShopInspector.Web.csproj -c Release -o /out


# ===== RUNTIME STAGE =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Bind ASP.NET Core to the Render PORT
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copy published output from build stage
COPY --from=build /out .

# Start the app
ENTRYPOINT ["dotnet", "ShopInspector.Web.dll"]
