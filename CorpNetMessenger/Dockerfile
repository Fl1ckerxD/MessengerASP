FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ["CorpNetMessenger/CorpNetMessenger.csproj", "CorpNetMessenger/"]
RUN dotnet restore "CorpNetMessenger/CorpNetMessenger.csproj"

COPY ./CorpNetMessenger ./CorpNetMessenger
WORKDIR /app/CorpNetMessenger
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/CorpNetMessenger/out .

ENTRYPOINT ["dotnet", "CorpNetMessenger.dll"]