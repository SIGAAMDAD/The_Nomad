#!/bin/sh
echo -ne '\033c\033]0;TheNomad\a'
base_path="$(dirname "$(realpath "$0")")"
"$base_path/TheNomad.x86_64" "$@"
