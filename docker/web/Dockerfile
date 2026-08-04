FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Ruptura.Shared/Ruptura.Shared.csproj", "src/Ruptura.Shared/"]
COPY ["src/Ruptura.Web/Ruptura.Web.csproj", "src/Ruptura.Web/"]
RUN dotnet restore "src/Ruptura.Web/Ruptura.Web.csproj"

COPY src/Ruptura.Shared/ src/Ruptura.Shared/
COPY src/Ruptura.Web/ src/Ruptura.Web/
RUN dotnet publish "src/Ruptura.Web/Ruptura.Web.csproj" -c Release -o /app/publish --no-restore

FROM nginx:1.27-alpine AS runtime

COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY docker/web/nginx.conf /etc/nginx/conf.d/default.conf
COPY docker/web/entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
