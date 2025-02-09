FROM ubuntu:latest AS base
ENV TZ=Etc/Utc
ENV DEBIAN_FRONTEND=noninteractive
RUN apt update
RUN apt install -y libcurl4
RUN apt install -y dotnet-runtime-8.0
RUN apt install -y tzdata
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM ubuntu:latest AS build
COPY . /src
RUN apt update
RUN apt install -y libcurl4

RUN apt install -y wget
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
RUN chmod +x ./dotnet-install.sh
RUN ./dotnet-install.sh --channel 9.0
RUN export PATH="$PATH:$HOME/.dotnet"

RUN dotnet publish /src/FFXIVVenues.Veni.csproj -c Release -o /src/build

FROM base AS final
WORKDIR /app
COPY --from=build /src/build .
ENTRYPOINT ["dotnet", "FFXIVVenues.Veni.dll"]
