# From repository root: docker build -f src/RaCeDay/Dockerfile -t raceday-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/RaCeDay/RaCeDay.csproj src/RaCeDay/
RUN dotnet restore src/RaCeDay/RaCeDay.csproj
COPY src/RaCeDay/ src/RaCeDay/
RUN dotnet publish src/RaCeDay/RaCeDay.csproj -c Release --no-restore -o /published
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /published .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "RaCeDay.dll"]
