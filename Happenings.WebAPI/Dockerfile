FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Happenings.WebAPI/Happenings.WebAPI.csproj", "Happenings.WebAPI/"]
COPY ["Happenings.Services/Happenings.Services.csproj", "Happenings.Services/"]
COPY ["Happenings.Model/Happenings.Model.csproj", "Happenings.Model/"]
RUN dotnet restore "Happenings.WebAPI/Happenings.WebAPI.csproj"
COPY . .
WORKDIR "/src/Happenings.WebAPI"
RUN dotnet build "Happenings.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Happenings.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Happenings.WebAPI.dll"]