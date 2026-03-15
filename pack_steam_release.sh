#!/bin/sh

zip -r -9 TheNomad.x64.zip data_The\ Nomad_windows_x86_64 TheNomad.x64.pck TheNomad.x64.exe *.dll steam_appid.txt addons/BugReporter/webhook.cfg
zip -r -9 TheNomad.x86_64.zip data_The\ Nomad_linuxbsd_x86_64 TheNomad.pck TheNomad.x86_64 *.so steam_appid.txt addons/BugReporter/webhook.cfg
