# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["Social_Mini_App/Social_Mini_App.csproj", "Social_Mini_App/"]
RUN dotnet restore "Social_Mini_App/Social_Mini_App.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Social_Mini_App"
RUN dotnet build "Social_Mini_App.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "Social_Mini_App.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose the port Render uses (Render sets the PORT environment variable)
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Social_Mini_App.dll"]
