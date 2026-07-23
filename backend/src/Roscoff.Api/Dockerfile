FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Roscoff.Api/Roscoff.Api.csproj", "src/Roscoff.Api/"]
COPY ["src/Roscoff.Application/Roscoff.Application.csproj", "src/Roscoff.Application/"]
COPY ["src/Roscoff.Core/Roscoff.Core.csproj", "src/Roscoff.Core/"]
COPY ["src/Roscoff.Infrastructure/Roscoff.Infrastructure.csproj", "src/Roscoff.Infrastructure/"]
RUN dotnet restore "src/Roscoff.Api/Roscoff.Api.csproj"
COPY . .
WORKDIR "/src/src/Roscoff.Api"
RUN dotnet build "./Roscoff.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Roscoff.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Roscoff.Api.dll"]
