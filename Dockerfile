FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY vulnerablewebpage.csproj ./
RUN dotnet restore ./vulnerablewebpage.csproj
COPY . ./
RUN dotnet publish ./vulnerablewebpage.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "vulnerablewebpage.dll"]
