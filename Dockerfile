# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env

# Set working directory
WORKDIR /src

# Copy project files
COPY SpecklePowerPivotForRevit/ ./

# Restore dependencies
RUN dotnet restore --use-current-runtime --runtime linux-x64

# Publish application
RUN dotnet publish --use-current-runtime --runtime linux-x64 --self-contained false --no-restore -o /publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime

# Set working directory
WORKDIR /publish

# Copy published application
COPY --from=build-env /publish .
