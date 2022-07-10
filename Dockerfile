#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG NUGET_REPO_PASSWORD
COPY . /app/
RUN echo ${NUGET_REPO_PASSWORD}
RUN dotnet nuget add source https://nuget.pkg.github.com/FFXIVVenues/index.json --username kana-ki --password ${NUGET_REPO_PASSWORD} --store-password-in-clear-text
RUN dotnet publish /app/FFXIVVenues.Veni.csproj -c Release -o /runtime/
WORKDIR /runtime/

ENTRYPOINT ["dotnet", "FFXIVVenues.Veni.dll"]