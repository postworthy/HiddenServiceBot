#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

RUN apt update
RUN apt install gnupg2 tor nginx procps --yes
RUN curl https://helm.baltorepo.com/organization/signing.asc | apt-key add -
RUN apt install apt-transport-https --yes
RUN echo "deb https://baltocdn.com/helm/stable/debian/ all main" | tee /etc/apt/sources.list.d/helm-stable-debian.list
RUN apt update
RUN apt install helm

#RUN sed -i 's/#HiddenServiceDir \/var\/lib\/tor\/hidden_service\//HiddenServiceDir \/var\/lib\/tor\/hidden_service\//g' /etc/tor/torrc
#RUN sed -i 's/#HiddenServicePort 80 127.0.0.1:80/HiddenServicePort 80 127.0.0.1:8080/g' /etc/tor/torrc

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["HiddenServiceBot/HiddenServiceBot.csproj", "HiddenServiceBot/"]
RUN dotnet restore "HiddenServiceBot/HiddenServiceBot.csproj"
COPY . .
WORKDIR "/src/HiddenServiceBot"
RUN dotnet build "HiddenServiceBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HiddenServiceBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "HiddenServiceBot.dll"]

