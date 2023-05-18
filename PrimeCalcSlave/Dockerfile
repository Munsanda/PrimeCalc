#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
EXPOSE 5672
EXPOSE 15672

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PrimeCalcSlave/PrimeCalcSlave.csproj", "PrimeCalcSlave/"]
RUN dotnet restore "PrimeCalcSlave/PrimeCalcSlave.csproj"
COPY . .
WORKDIR "/src/PrimeCalcSlave"
RUN dotnet build "PrimeCalcSlave.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PrimeCalcSlave.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PrimeCalcSlave.dll"]