FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /source

COPY ./SteadybitFaultInjection/*.csproj ./SteadybitFaultInjection/
RUN dotnet restore ./SteadybitFaultInjection/*.csproj

COPY ./SteadybitAspNet/*.csproj ./SteadybitAspNet/
RUN dotnet restore ./SteadybitAspNet/*.csproj

COPY . .
RUN dotnet publish ./SteadybitAspNet/SteadybitAspNet.csproj  -o /app


FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["./SteadybitAspNet"]
