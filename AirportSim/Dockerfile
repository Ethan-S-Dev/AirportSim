FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["AirportSim.Api/AirportSim.Api.csproj", "AirportSim.Api/"]
COPY ["AirportSim.Application/AirportSim.Application.csproj", "AirportSim.Application/"]
COPY ["AirportSim.Domain/AirportSim.Domain.csproj", "AirportSim.Domain/"]
COPY ["AirportSim.Infra.Data/AirportSim.Infra.Data.csproj", "AirportSim.Infra.Data/"]
COPY ["AirportSim.Infra.IoC/AirportSim.Infra.IoC.csproj", "AirportSim.Infra.IoC/"]
RUN dotnet restore "AirportSim.Api/AirportSim.Api.csproj"
COPY . .
WORKDIR "/src/AirportSim.Api"
RUN dotnet build "AirportSim.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AirportSim.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir /data
ENTRYPOINT ["dotnet", "AirportSim.Api.dll"]