FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/AgileFlow.slnx backend/AgileFlow.slnx
COPY backend/API/API.csproj backend/API/API.csproj
COPY backend/Application/Application.csproj backend/Application/Application.csproj
COPY backend/Domain/Domain.csproj backend/Domain/Domain.csproj
COPY backend/Infrastructure/Infrastructure.csproj backend/Infrastructure/Infrastructure.csproj
RUN dotnet restore backend/API/API.csproj

COPY backend backend
RUN dotnet publish backend/API/API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "API.dll"]
