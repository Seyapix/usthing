FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Timetable.sln ./
COPY Timetable.Api/Timetable.Api.csproj Timetable.Api/
RUN dotnet restore Timetable.sln

COPY . .
RUN dotnet publish Timetable.Api/Timetable.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
USER root
RUN mkdir -p /data && chown "$APP_UID:$APP_UID" /data
USER $APP_UID
ENTRYPOINT ["dotnet", "Timetable.Api.dll"]
