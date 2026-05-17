FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CustomerService.API/CustomerService.API.csproj", "CustomerService.API/"]
COPY ["CustomerService.Application/CustomerService.Application.csproj", "CustomerService.Application/"]
COPY ["CustomerService.Domain/CustomerService.Domain.csproj", "CustomerService.Domain/"]
COPY ["CustomerService.Infrastructure/CustomerService.Infrastructure.csproj", "CustomerService.Infrastructure/"]
COPY ["BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

RUN dotnet restore "CustomerService.API/CustomerService.API.csproj"

COPY . .
WORKDIR "/src/CustomerService.API"
RUN dotnet publish "CustomerService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CustomerService.API.dll"]
