
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

# Install gRPC packages for Alpine
RUN apk add --no-cache grpc-plugins

# Set environment variables for the tools
ENV PROTOBUF_PROTOC=/usr/bin/protoc
ENV GRPC_PROTOC_PLUGIN=/usr/bin/grpc_csharp_plugin

WORKDIR /src
COPY ["Billing/src/Billing.Api/Billing.Api.csproj", "Billing/src/Billing.Api/"]
COPY ["Billing/src/Billing/Billing.csproj", "Billing/src/Billing/"]
COPY ["Platform/src/Operations.ServiceDefaults/Operations.ServiceDefaults.csproj", "Platform/src/Operations.ServiceDefaults/"]
COPY ["Platform/src/Operations.ServiceDefaults.Api/Operations.ServiceDefaults.Api.csproj", "Platform/src/Operations.ServiceDefaults.Api/"]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
RUN dotnet restore "Billing/src/Billing.Api/Billing.Api.csproj"
COPY . .
WORKDIR "/src/Billing/src/Billing.Api"
RUN dotnet build "Billing.Api.csproj" -c $BUILD_CONFIGURATION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Billing.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Billing.Api.dll"]
