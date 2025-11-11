# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Install Node.js (for Angular build)
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy everything into the container
COPY . .

# Restore and build .NET backend
WORKDIR /app/AllAccessApp.API
RUN dotnet restore
RUN dotnet publish -c Release -o /out

# Build Angular frontend
WORKDIR /app/AllAccessApp.Frontend
RUN npm install
RUN npm run build -- --configuration production

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out .

EXPOSE 80
ENTRYPOINT ["dotnet", "AllAccessApp.API.dll"]
