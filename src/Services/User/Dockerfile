# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Services/User/User.csproj", "Services/User/"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "BuildingBlocks/Common/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/"]

# Restore dependencies
RUN dotnet restore "Services/User/User.csproj"

# Copy remaining files and build
COPY src/Services/User/. Services/User/
COPY src/BuildingBlocks/Common/. BuildingBlocks/Common/
COPY src/BuildingBlocks/EventBus/. BuildingBlocks/EventBus/

WORKDIR "/src/Services/User"
RUN dotnet build "User.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "User.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User.dll"]
