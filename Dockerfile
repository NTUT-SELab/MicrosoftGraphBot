#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MicrosoftGraphAPIBot/MicrosoftGraphAPIBot.csproj", "MicrosoftGraphAPIBot/"]
RUN dotnet restore "MicrosoftGraphAPIBot/MicrosoftGraphAPIBot.csproj"
COPY . .
WORKDIR "/src/MicrosoftGraphAPIBot"
RUN dotnet build "MicrosoftGraphAPIBot.csproj" -c Release -o /app/build

FROM build AS publish
ARG RELEASE_VERSION
RUN dotnet publish "MicrosoftGraphAPIBot.csproj" -c Release -o /app/publish /p:Version=$RELEASE_VERSION

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicrosoftGraphAPIBot.dll"]
