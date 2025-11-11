# Stage 1: Build .NET + Angular
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ✅ Install Node.js (for Angular build)
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    node -v && npm -v

# Copy everything
COPY . .

# Restore .NET dependencies
WORKDIR /src/AllAccessApp.API
RUN dotnet restore "AllAccessApp.API.csproj"

# ✅ Build Angular (path corrected)
WORKDIR /src/AllAccessApp.Frontend
RUN npm install
RUN npm run build -- --configuration production

# Publish .NET app
WORKDIR /src/AllAccessApp.API
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "AllAccessApp.API.dll"]
