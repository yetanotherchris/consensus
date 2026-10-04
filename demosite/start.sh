#!/bin/sh
set -eu

dotnet Consensus.Api.dll &
app_pid=$!

nginx -g 'daemon off;' &
nginx_pid=$!

shutdown() {
    kill -TERM "$nginx_pid" "$app_pid" 2>/dev/null || true
    wait "$nginx_pid" 2>/dev/null || true
    wait "$app_pid" 2>/dev/null || true
}

trap 'shutdown; exit 0' INT TERM

wait "$nginx_pid" || true
kill -TERM "$app_pid" 2>/dev/null || true
wait "$app_pid" 2>/dev/null || true
