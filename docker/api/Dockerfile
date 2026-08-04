FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Ruptura.Domain/Ruptura.Domain.csproj", "src/Ruptura.Domain/"]
COPY ["src/Ruptura.Application/Ruptura.Application.csproj", "src/Ruptura.Application/"]
COPY ["src/Ruptura.Infrastructure/Ruptura.Infrastructure.csproj", "src/Ruptura.Infrastructure/"]
COPY ["src/Ruptura.Shared/Ruptura.Shared.csproj", "src/Ruptura.Shared/"]
COPY ["src/Ruptura.API/Ruptura.API.csproj", "src/Ruptura.API/"]
RUN dotnet restore "src/Ruptura.API/Ruptura.API.csproj"

COPY src/ src/
RUN dotnet publish "src/Ruptura.API/Ruptura.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=build --chown=appuser:appgroup /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000
ENTRYPOINT ["dotnet", "Ruptura.API.dll"]
