FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["AirportSim.Simulator.Api/AirportSim.Simulator.Api.csproj", "AirportSim.Simulator.Api/"]
COPY ["AirportSim.Simulator.Application/AirportSim.Simulator.Application.csproj", "AirportSim.Simulator.Application/"]
COPY ["AirportSim.Simulator.Domain/AirportSim.Simulator.Domain.csproj", "AirportSim.Simulator.Domain/"]
COPY ["AirportSim.Simulator.Infra.AirportControl/AirportSim.Simulator.Infra.AirportControl.csproj", "AirportSim.Simulator.Infra.AirportControl/"]
COPY ["AirportSim.Simulator.Infra.IoC/AirportSim.Simulator.Infra.IoC.csproj", "AirportSim.Simulator.Infra.IoC/"]
COPY ["AirportSim.Simulator.Infra.Random/AirportSim.Simulator.Infra.Random.csproj", "AirportSim.Simulator.Random.IoC/"]
RUN dotnet restore "AirportSim.Simulator.Api/AirportSim.Simulator.Api.csproj"
COPY . .
WORKDIR "/src/AirportSim.Simulator.Api"
RUN dotnet build "AirportSim.Simulator.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AirportSim.Simulator.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir /data
ENTRYPOINT ["dotnet", "AirportSim.Simulator.Api.dll"]