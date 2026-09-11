FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /InternshipService

COPY InternshipService/InternshipService.csproj .
RUN dotnet restore

COPY InternshipService .

RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "InternshipService.dll"]
