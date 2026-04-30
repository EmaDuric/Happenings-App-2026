FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Happenings.Subscriber/Happenings.Subscriber.csproj", "Happenings.Subscriber/"]
COPY ["Happenings.Services/Happenings.Services.csproj", "Happenings.Services/"]
COPY ["Happenings.Model/Happenings.Model.csproj", "Happenings.Model/"]
RUN dotnet restore "Happenings.Subscriber/Happenings.Subscriber.csproj"
COPY . .
WORKDIR "/src/Happenings.Subscriber"
RUN dotnet build "Happenings.Subscriber.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Happenings.Subscriber.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Happenings.Subscriber.dll"]