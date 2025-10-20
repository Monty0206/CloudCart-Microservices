FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["CloudCartMicroservices.sln", "./"]
COPY ["src/BuildingBlocks/Common/Common.csproj", "src/BuildingBlocks/Common/"]
COPY ["src/BuildingBlocks/EventBus/EventBus.csproj", "src/BuildingBlocks/EventBus/"]
COPY ["src/Services/ShoppingCart/ShoppingCart.csproj", "src/Services/ShoppingCart/"]
RUN dotnet restore "src/Services/ShoppingCart/ShoppingCart.csproj"
COPY . .
WORKDIR "/src/src/Services/ShoppingCart"
RUN dotnet build "ShoppingCart.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShoppingCart.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShoppingCart.dll"]
