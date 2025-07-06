# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
WORKDIR /source

COPY --link . .
RUN dotnet restore -a $TARGETARCH
RUN dotnet publish ./src/Borealis.Web/Borealis.Web.csproj -a $TARGETARCH --no-restore -o /app

# Build frontend
FROM node:24-slim AS frontend
WORKDIR /source

COPY --link ./src/Borealis.Frontend .
RUN yarn
RUN yarn build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

RUN mkdir -p Database
RUN mkdir -p LuceneIndex
RUN mkdir -p wwwroot
RUN mkdir -p accounts

COPY --link --from=build /app .
COPY --link --from=frontend /source/artifacts ./wwwroot

USER $APP_UID

EXPOSE 8080/tcp
EXPOSE 8081/tcp

ENTRYPOINT ["./Borealis.Web"]
