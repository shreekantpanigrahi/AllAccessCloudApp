# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install Node.js (for Angular build)
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy everything into container
COPY . .

# Restore and build .NET backend
WORKDIR /src/src/AllAccessApp.API
RUN dotnet restore "AllAccessApp.API.csproj"
RUN dotnet publish "AllAccessApp.API.csproj" -c Release -o /app/publish

# Build Angular frontend
WORKDIR /src/src/AllAccessApp.Frontend
RUN npm install
RUN npm run build -- --configuration production

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "AllAccessApp.API.dll"]
