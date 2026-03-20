# --- Stage 1: Build ---
# Start with a full .NET SDK image (has everything needed to compile)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy your source code into the container
COPY . .

# Compile and publish the app into /app/out
RUN dotnet publish -c Release -o out


# --- Stage 2: Runtime ---
# Start fresh with a smaller image (no SDK, just the runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copy only the compiled output from Stage 1
COPY --from=build /app/out .

# Tell ASP.NET to listen on Railway's dynamic port
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT

# The command that runs when the container starts
ENTRYPOINT ["dotnet", "YourProjectName.dll"]