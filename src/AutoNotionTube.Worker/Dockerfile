FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/AutoNotionTube.Worker/AutoNotionTube.Worker.csproj", "src/AutoNotionTube.Worker/"]
COPY ["src/AutoNotionTube.Core/AutoNotionTube.Core.csproj", "src/AutoNotionTube.Core/"]
COPY ["src/AutoNotionTube.Domain/AutoNotionTube.Domain.csproj", "src/AutoNotionTube.Domain/"]
COPY ["src/AutoNotionTube.Infrastructure/AutoNotionTube.Infrastructure.csproj", "src/AutoNotionTube.Infrastructure/"]
RUN dotnet restore "src/AutoNotionTube.Worker/AutoNotionTube.Worker.csproj"
COPY . .
WORKDIR "/src/src/AutoNotionTube.Worker"
RUN dotnet build "AutoNotionTube.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AutoNotionTube.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoNotionTube.Worker.dll"]
