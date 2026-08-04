#!/bin/sh
set -e

# Generate config.json from environment variables at container startup.
# This allows runtime configuration of the Blazor WASM app without rebuilding.
# In local development (without Docker), config.json keeps its default values.
CONFIG_FILE="/usr/share/nginx/html/config.json"

cat > "$CONFIG_FILE" << EOF
{
  "ApiBaseUrl": "${API_BASE_URL}"
}
EOF

exec nginx -g "daemon off;"
