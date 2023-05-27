FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG NUGET_REPO_PASSWORD
COPY . /src
RUN dotnet nuget add source https://nuget.pkg.github.com/FFXIVVenues/index.json --username kana-ki --password ${NUGET_REPO_PASSWORD} --store-password-in-clear-text
RUN dotnet publish /src/FFXIVVenues.Veni.csproj -c Release -o /src/build

FROM base AS final
WORKDIR /app
COPY --from=build /src/build .
ENTRYPOINT ["dotnet", "FFXIVVenues.Veni.dll"]