#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

RUN apt update
RUN apt install gnupg2 tor nginx procps sudo --yes
RUN curl https://helm.baltorepo.com/organization/signing.asc | apt-key add -
RUN apt install apt-transport-https --yes
RUN echo "deb https://baltocdn.com/helm/stable/debian/ all main" | tee /etc/apt/sources.list.d/helm-stable-debian.list
RUN apt update
RUN apt install helm

RUN groupadd -r botuser && useradd --create-home -r -g botuser botuser
RUN echo 'botuser ALL=(ALL) NOPASSWD: /usr/sbin/nginx' >> /etc/sudoers #allows botuser to launch nginx w/o being root
RUN chown botuser /etc/tor/torrc
RUN chown botuser /var/lib/tor/
RUN chown botuser /etc/nginx/conf.d/
USER botuser

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

