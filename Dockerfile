# 使用官方 .NET 6 映像檔作為基礎
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# 使用 .NET 6 SDK 映像檔作為建構階段
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# 最終執行階段
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MessengerBot.dll"]