FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY cmsOrg.sln .
COPY src/cmsOrg.Domain/cmsOrg.Domain.csproj src/cmsOrg.Domain/
COPY src/cmsOrg.Application/cmsOrg.Application.csproj src/cmsOrg.Application/
COPY src/cmsOrg.Infrastructure/cmsOrg.Infrastructure.csproj src/cmsOrg.Infrastructure/
COPY src/cmsOrg.API/cmsOrg.API.csproj src/cmsOrg.API/
RUN dotnet restore

COPY . .
RUN dotnet publish src/cmsOrg.API/cmsOrg.API.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "cmsOrg.API.dll"]
