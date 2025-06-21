FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine

RUN dotnet tool install -g docfx
ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /app
ENTRYPOINT [ "docfx" ]