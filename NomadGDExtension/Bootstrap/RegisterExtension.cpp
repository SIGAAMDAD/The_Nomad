#include "RegisterExtension.hpp"
#include "godot_cpp/core/class_db.hpp"

#include <Gameplay/NMDGameStateManager.h>

#include <gdextension_interface.h>
#include <godot_cpp/godot.hpp>

using namespace godot;

void initialize_gdextension_template( const ModuleInitializationLevel level )
{
	if ( level != MODULE_INITIALIZATION_LEVEL_SCENE ) {
		return;
	}

	// Register classes implemented under Source/ here.
	//
	// Example:
	//   #include "Gameplay/MyNode.hpp"
	//   GDREGISTER_CLASS(MyNode);
}

void uninitialize_gdextension_template( const ModuleInitializationLevel level )
{
	if ( level != MODULE_INITIALIZATION_LEVEL_SCENE ) {
		return;
	}
}

extern "C" {

GDExtensionBool GDE_EXPORT GDEXTENSION_ENTRY_SYMBOL(
    GDExtensionInterfaceGetProcAddress get_proc_address,
    GDExtensionClassLibraryPtr library,
    GDExtensionInitialization *initialization )
{
	GDExtensionBinding::InitObject init_object(
	    get_proc_address,
	    library,
	    initialization );

	init_object.register_initializer( initialize_gdextension_template );
	init_object.register_terminator( uninitialize_gdextension_template );
	init_object.set_minimum_library_initialization_level(
	    MODULE_INITIALIZATION_LEVEL_SCENE );

	return init_object.init();
}

} // extern "C"
