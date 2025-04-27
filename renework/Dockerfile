# Use the .NET 8.0 SDK to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["renework.csproj", "./"]
RUN dotnet restore

# copy the rest and publish
COPY . .
RUN dotnet publish -c Release -o /app

# use the .NET 8.0 ASP.NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# copy published output
COPY --from=build /app .

# listen on port 5000
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "renework.dll"]
