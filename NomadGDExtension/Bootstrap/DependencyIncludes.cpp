// This translation unit intentionally includes one public header from every
// native dependency. It turns the dependency include-path contract into a
// build check instead of relying on machine-specific IDE include settings.

#include <EABase/eabase.h>
#include <EASTL/vector.h>
#include <Framework/Core/Platform/NMDPCH.h>
#include <godot_cpp/classes/node.hpp>

static_assert(sizeof(godot::Node *) == sizeof(void *));
