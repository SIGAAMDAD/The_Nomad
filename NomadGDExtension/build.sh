#!/bin/sh
# Invoke with sh build.sh [Debug|Release] [additional CMake configure arguments].
set -eu
cd -- "$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
configuration=${1:-Debug}
if [ "$#" -gt 0 ]; then shift; fi
case "$configuration" in
    Debug|debug) preset=linux-debug ;;
    Release|release) preset=linux-release ;;
    *) echo "Usage: sh build.sh [Debug|Release] [CMake configure arguments]" >&2; exit 2 ;;
esac
cmake --preset "$preset" "$@"
cmake --build --preset "$preset" --parallel "${NOMAD_BUILD_JOBS:-8}"
