# From repository root: docker build -f src/RaCeDay.Portal/Dockerfile -t raceday-portal .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY src/RaCeDay.Portal/RaCeDay.Portal.csproj src/RaCeDay.Portal/
RUN dotnet restore src/RaCeDay.Portal/RaCeDay.Portal.csproj
COPY src/RaCeDay.Portal/ src/RaCeDay.Portal/
RUN dotnet publish src/RaCeDay.Portal/RaCeDay.Portal.csproj -c Release --no-restore -o /published
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /published .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet","RaCeDay.Portal.dll"]
