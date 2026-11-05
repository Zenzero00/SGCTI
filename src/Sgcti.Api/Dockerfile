FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/Sgcti.Api/Sgcti.Api.csproj", "src/Sgcti.Api/"]
COPY ["src/Sgcti.Core/Sgcti.Core.csproj", "src/Sgcti.Core/"]
COPY ["src/Sgcti.Infrastructure/Sgcti.Infrastructure.csproj", "src/Sgcti.Infrastructure/"]
RUN dotnet restore "src/Sgcti.Api/Sgcti.Api.csproj"
COPY . .
WORKDIR "/src/src/Sgcti.Api"
RUN dotnet build "Sgcti.Api.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "Sgcti.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sgcti.Api.dll"]
