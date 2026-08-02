# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["1-Payment.Api/1-Payment.Api.csproj", "1-Payment.Api/"]
COPY ["2-Payment.Application/2-Payment.Application.csproj", "2-Payment.Application/"]
COPY ["2-Payment.Domain/4-Payment.Domain.csproj", "2-Payment.Domain/"]
COPY ["3-Payment.Infrastructure/3-Payment.Infrastructure.csproj", "3-Payment.Infrastructure/"]

RUN dotnet restore "1-Payment.Api/1-Payment.Api.csproj"

# Copy the rest of the source code
COPY . .

WORKDIR "/src/1-Payment.Api"

RUN dotnet publish "1-Payment.Api.csproj" \
    -c Release \
    -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

EXPOSE 8082

ENV ASPNETCORE_URLS=http://+:8082

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "1-Payment.Api.dll"]
