#!/bin/sh
set -e

# Replace env variable placeholders in config.json at container startup.
# This allows runtime configuration of the Blazor WASM app without rebuilding.
CONFIG_FILE="/usr/share/nginx/html/config.json"

envsubst < "$CONFIG_FILE" > "$CONFIG_FILE.tmp" && mv "$CONFIG_FILE.tmp" "$CONFIG_FILE"

exec nginx -g "daemon off;"
