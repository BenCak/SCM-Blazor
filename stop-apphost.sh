#!/usr/bin/env bash

set -u

APPHOST_PATTERN='[S]CM3\.AppHost'
APPHOST_PORTS=(15088 17060 19200 20253 21069 22220)

mapfile -t apphost_pids < <(pgrep -f "$APPHOST_PATTERN" || true)

if ((${#apphost_pids[@]} > 0)); then
    echo "Stopping SCM3.AppHost processes: ${apphost_pids[*]}"
    kill "${apphost_pids[@]}" 2>/dev/null || true

    for _ in {1..10}; do
        remaining_pids=()
        for pid in "${apphost_pids[@]}"; do
            kill -0 "$pid" 2>/dev/null && remaining_pids+=("$pid")
        done

        ((${#remaining_pids[@]} == 0)) && break
        sleep 0.5
    done

    if ((${#remaining_pids[@]} > 0)); then
        echo "Force-stopping remaining processes: ${remaining_pids[*]}"
        kill -9 "${remaining_pids[@]}" 2>/dev/null || true
    fi
else
    echo "No SCM3.AppHost process found."
fi

if command -v fuser >/dev/null 2>&1; then
    for port in "${APPHOST_PORTS[@]}"; do
        if fuser "${port}/tcp" >/dev/null 2>&1; then
            echo "Clearing process using TCP port $port"
            fuser -k "${port}/tcp" >/dev/null 2>&1 || true
        fi
    done
fi

echo "SCM3.AppHost cleanup complete."
