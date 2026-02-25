# ---------------------------
# Build Stage (.NET 10 SDK)
# ---------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy entire solution
COPY . .

# Restore and publish Web project
RUN dotnet restore src/ContainerManagement.Web/ContainerManagement.Web.csproj
RUN dotnet publish src/ContainerManagement.Web/ContainerManagement.Web.csproj \
    -c Release -o /app/publish

# ---------------------------
# Runtime Stage (.NET 10 ASP.NET)
# ---------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Render dynamic port support
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 10000

ENTRYPOINT ["dotnet", "ContainerManagement.Web.dll"]