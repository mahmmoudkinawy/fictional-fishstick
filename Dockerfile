# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all files and publish
COPY . ./
RUN dotnet publish -c Release -o /out

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Copy published output
COPY --from=build /out ./

# Environment variables (if needed)
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Run the worker
ENTRYPOINT ["dotnet", "OSS.dll"]
