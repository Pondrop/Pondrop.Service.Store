#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Pondrop.Service.Store.Api/Pondrop.Service.Store.Api.csproj", "src/Pondrop.Service.Store.Api/"]
COPY ["src/Pondrop.Service.Store.Application/Pondrop.Service.Store.Application.csproj", "src/Pondrop.Service.Store.Application/"]
COPY ["src/Pondrop.Service.Store.Domain/Pondrop.Service.Store.Domain.csproj", "src/Pondrop.Service.Store.Domain/"]
COPY ["src/Pondrop.Service.Store.Infrastructure/Pondrop.Service.Store.Infrastructure.csproj", "src/Pondrop.Service.Store.Infrastructure/"]
RUN dotnet restore "src/Pondrop.Service.Store.Api/Pondrop.Service.Store.Api.csproj"
COPY . .
WORKDIR "/src/src/Pondrop.Service.Store.Api"
RUN dotnet build "Pondrop.Service.Store.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pondrop.Service.Store.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pondrop.Service.Store.Api.dll"]