FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AuthService.API/AuthService.API.csproj", "AuthService.API/"]
COPY ["AuthService.Application/AuthService.Application.csproj", "AuthService.Application/"]
COPY ["AuthService.Domain/AuthService.Domain.csproj", "AuthService.Domain/"]
COPY ["AuthService.Infrastructure/AuthService.Infrastructure.csproj", "AuthService.Infrastructure/"]
COPY ["BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

RUN dotnet restore "AuthService.API/AuthService.API.csproj"

COPY . .
WORKDIR "/src/AuthService.API"
RUN dotnet publish "AuthService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuthService.API.dll"]
