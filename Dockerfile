
# Build ứng dụng ASP.NET Core 8
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Sao chép mã nguồn
COPY . .

# Publish project
RUN dotnet publish "CeramicsArt/CeramicsArt.csproj" \
    -c Release \
    -o /app/publish

# Image chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

# Cấu hình cổng cho Render
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "CeramicsArt.dll"]
