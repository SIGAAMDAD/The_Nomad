#!/bin/sh
printf '\033c\033]0;%s\a' The Nomad
base_path="$(dirname "$(realpath "$0")")"
"$base_path/TheNomad.x86_64" "$@"
