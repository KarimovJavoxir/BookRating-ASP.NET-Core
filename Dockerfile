# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

USER root
WORKDIR /src

# Copy solution file
COPY ["BookRatingSystem.sln", "./"]

# Copy project files for restore caching
COPY ["src/BookRatingSystem.Api/BookRatingSystem.Api.csproj", "src/BookRatingSystem.Api/"]
COPY ["src/BookRatingSystem.Application/BookRatingSystem.Application.csproj", "src/BookRatingSystem.Application/"]
COPY ["src/BookRatingSystem.Domain/BookRatingSystem.Domain.csproj", "src/BookRatingSystem.Domain/"]
COPY ["src/BookRatingSystem.Infrastructure/BookRatingSystem.Infrastructure.csproj", "src/BookRatingSystem.Infrastructure/"]

# Copy full source
COPY . .

# Publish startup project
WORKDIR /src/src/BookRatingSystem.Api

RUN dotnet publish "BookRatingSystem.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-self-contained \
    -r linux-x64 --disable-parallel

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

RUN apt-get update && apt-get install -y libgssapi-krb5-2

WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BookRatingSystem.Api.dll"]