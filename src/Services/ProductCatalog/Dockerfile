FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["CloudCartMicroservices.sln", "./"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "src/BuildingBlocks/Common/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/Services/ProductCatalog/ProductCatalog.csproj", "src/Services/ProductCatalog/"]

# Restore dependencies
RUN dotnet restore "src/Services/ProductCatalog/ProductCatalog.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Services/ProductCatalog"
RUN dotnet build "ProductCatalog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductCatalog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductCatalog.dll"]
