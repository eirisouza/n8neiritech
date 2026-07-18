FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Api/n8neiritech.Api.csproj", "src/Api/"]
COPY ["src/Application/n8neiritech.Application.csproj", "src/Application/"]
COPY ["src/Domain/n8neiritech.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/n8neiritech.Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/Api/n8neiritech.Api.csproj"

COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "n8neiritech.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "n8neiritech.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

RUN mkdir -p /app/logs /app/uploads

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "n8neiritech.Api.dll"]
