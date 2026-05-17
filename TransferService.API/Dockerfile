FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TransferService.API/TransferService.API.csproj", "TransferService.API/"]
COPY ["TransferService.Application/TransferService.Application.csproj", "TransferService.Application/"]
COPY ["TransferService.Domain/TransferService.Domain.csproj", "TransferService.Domain/"]
COPY ["TransferService.Infrastructure/TransferService.Infrastructure.csproj", "TransferService.Infrastructure/"]
COPY ["BuildingBlocks/BuildingBlocks.csproj", "BuildingBlocks/"]

RUN dotnet restore "TransferService.API/TransferService.API.csproj"

COPY . .
WORKDIR "/src/TransferService.API"
RUN dotnet publish "TransferService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TransferService.API.dll"]
