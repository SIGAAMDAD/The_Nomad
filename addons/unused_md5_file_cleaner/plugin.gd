@tool
extends EditorPlugin


const PLUGIN_NAME = "Unused MD5 File Cleaner"
const TOOL_MENU_NAME = "Clean Up Unused MD5 Files"


func _enter_tree():
	add_tool_menu_item(TOOL_MENU_NAME, clean_up_unused_md5_files)


func _exit_tree():
	remove_tool_menu_item(TOOL_MENU_NAME)


func clean_up_unused_md5_files() -> void:
	var dir = DirAccess.open(".godot/imported")
	var files = dir.get_files()

	var basename_counts = {}
	for file in files:
		basename_counts[file.get_basename()] = 0
	for file in files:
		basename_counts[file.get_basename()] += 1

	var remove_counts = 0
	for basename in basename_counts:
		if basename_counts[basename] == 1:
			var basename_md5 = basename + ".md5"
			if dir.file_exists(basename_md5):
				dir.remove(basename_md5)
				remove_counts += 1
				print('%s: deleted "%s"' % [PLUGIN_NAME, basename_md5])
	print('%s: deleted %d files' % [PLUGIN_NAME, remove_counts])
