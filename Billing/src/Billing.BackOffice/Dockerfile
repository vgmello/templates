FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Billing/src/Billing.BackOffice/Billing.BackOffice.csproj", "Billing/src/Billing.BackOffice/"]
COPY ["Billing/src/Billing/Billing.csproj", "Billing/src/Billing/"]
COPY ["Platform/src/Operations.ServiceDefaults/Operations.ServiceDefaults.csproj", "Platform/src/Operations.ServiceDefaults/"]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "Billing/src/Billing.BackOffice/Billing.BackOffice.csproj"
COPY . .
WORKDIR "/src/Billing/src/Billing.BackOffice"
RUN dotnet build "Billing.BackOffice.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Billing.BackOffice.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Billing.BackOffice.dll"]
