# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Install Node.js (for Angular build)
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy all files
COPY . .

# Restore and publish .NET backend
WORKDIR /app/AllAccessApp.API
RUN dotnet restore
RUN dotnet build -c Release
RUN dotnet publish -c Release -o /out

# Build Angular frontend
WORKDIR /app/AllAccessApp.Frontend
RUN npm install
RUN npm run build -- --configuration production

# ✅ Copy Angular build output to API wwwroot
RUN mkdir -p /app/AllAccessApp.API/wwwroot
RUN cp -r /app/AllAccessApp.Frontend/dist/all-access-app.frontend/browser/* /app/AllAccessApp.API/wwwroot/

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out .
COPY --from=build /app/AllAccessApp.API/wwwroot ./wwwroot

EXPOSE 80
ENTRYPOINT ["dotnet", "AllAccessApp.API.dll"]
