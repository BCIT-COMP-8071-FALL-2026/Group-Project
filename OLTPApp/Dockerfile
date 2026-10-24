
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

# Remove Windows-generated build assets
RUN rm -rf obj bin

# Restore packages inside the Linux container
RUN dotnet restore "OLTPApp.csproj"

RUN dotnet publish "OLTPApp.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OLTPApp.dll"]
