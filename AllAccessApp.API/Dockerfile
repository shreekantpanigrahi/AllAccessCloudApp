# Stage 1: Build .NET + Angular
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore "AllAccessApp.API.csproj"

# Build Angular frontend
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
