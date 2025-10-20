# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Services/Order/Order.csproj", "Services/Order/"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "BuildingBlocks/Common/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/"]

# Restore dependencies
RUN dotnet restore "Services/Order/Order.csproj"

# Copy remaining files and build
COPY src/Services/Order/. Services/Order/
COPY src/BuildingBlocks/Common/. BuildingBlocks/Common/
COPY src/BuildingBlocks/EventBus/. BuildingBlocks/EventBus/

WORKDIR "/src/Services/Order"
RUN dotnet build "Order.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Order.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.dll"]
