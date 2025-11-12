# ----------------------------
# ---- Build Stage ----
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Install Node.js (for Angular build)
RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Copy everything
COPY . .

# 1️⃣ Build Angular frontend
WORKDIR /app/AllAccessApp.Frontend
RUN npm install
RUN npm run build -- --configuration production

# 2️⃣ Copy Angular build into backend's wwwroot
WORKDIR /app/AllAccessApp.API
RUN mkdir -p wwwroot
RUN cp -r /app/AllAccessApp.Frontend/dist/* ./wwwroot/

# 3️⃣ Build and publish .NET backend (includes static files)
RUN dotnet restore
RUN dotnet publish -c Release -o /out

# ----------------------------
# ---- Runtime Stage ----
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /out ./

EXPOSE 80
ENTRYPOINT ["dotnet", "AllAccessApp.API.dll"]
