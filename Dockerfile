FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:6.0-jammy AS base
ENV TZ=Etc/Utc
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update && apt-get install -y tzdata
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0-jammy AS build
COPY . /src
RUN dotnet publish /src/FFXIVVenues.Veni.csproj -c Release -o /src/build

FROM base AS final
WORKDIR /app
COPY --from=build /src/build .
ENTRYPOINT ["dotnet", "FFXIVVenues.Veni.dll"]