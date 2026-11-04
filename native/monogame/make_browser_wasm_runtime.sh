#!/usr/bin/env sh
set -eu

configuration="${1:-Debug}"

case "$configuration" in
    Debug)
        preset="browser-wasm-debug"
        ;;
    Release)
        preset="browser-wasm-release"
        ;;
    *)
        echo "Unsupported configuration '$configuration'. Use Debug or Release." >&2
        exit 1
        ;;
esac

script_dir=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
browser_wasm_root="$script_dir/browser-wasm"

cd "$browser_wasm_root"
cmake --preset "$preset"
cmake --build --preset "$preset"
