# syntax=docker/dockerfile:1-labs
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

## Install gRPC tools
RUN apk add --no-cache grpc-plugins
ENV PROTOBUF_PROTOC=/usr/bin/protoc
ENV GRPC_PROTOC_PLUGIN=/usr/bin/grpc_csharp_plugin

WORKDIR /src

## Copy all project and solution files to restore dependencies
COPY --parents **/**/**/*.csproj .
COPY *.slnx Directory.Build.props Directory.Packages.props ./

## Restore packages - this layer is cached unless dependencies change
RUN dotnet restore --verbosity minimal

## Copy source code (this layer invalidates on any code change)
COPY . .

# Build & Publish
RUN dotnet publish "src/Billing.BackOffice/Billing.BackOffice.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    --verbosity minimal

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Billing.BackOffice.dll"]
