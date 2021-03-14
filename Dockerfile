FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY Vecc.GhostTemplating/Vecc.GhostTemplating.csproj .
RUN dotnet restore
COPY Vecc.GhostTemplating .
RUN dotnet build
RUN dotnet publish -c Release -o /pub

FROM mcr.microsoft.com/dotnet/runtime:5.0
WORKDIR /app
COPY --from=build /pub /app

ENTRYPOINT ["dotnet", "Vecc.GhostTemplating.dll"]