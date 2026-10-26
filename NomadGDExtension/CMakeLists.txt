cmake_minimum_required(VERSION 3.24)

if(CMAKE_SOURCE_DIR STREQUAL CMAKE_BINARY_DIR)
	message(FATAL_ERROR "Use an out-of-source build: cmake --preset msvc-debug or linux-debug")
endif()

# Also supports opening this folder directly as a CMake project.
if(NOT TARGET Nomad::Framework)
	project(NomadGameGDExtension LANGUAGES CXX)
	set(BUILD_TESTING OFF CACHE BOOL "" FORCE)
	set(NOMAD_BUILD_STANDALONE_GDEXTENSION ON CACHE BOOL "" FORCE)
	set(NOMAD_BUILD_GAME_EXTENSION OFF CACHE BOOL "" FORCE)
	add_subdirectory("../Source/NomadFramework/GDExtension" "nomad-framework")
endif()

string(TOLOWER "${CMAKE_SYSTEM_NAME}" NOMAD_PLATFORM_NAME)

set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

add_library(nomad_game_gdextension SHARED
	Bootstrap/RegisterExtension.cpp
	Bootstrap/DependencyIncludes.cpp
	Source/Gameplay/Player/NMDInventory.cpp
)

target_compile_features(nomad_game_gdextension PRIVATE cxx_std_20)
# The unfinished Story implementation files are not compilation inputs yet.
file(GLOB_RECURSE game_headers CONFIGURE_DEPENDS "Source/*.h")
target_sources(nomad_game_gdextension PRIVATE ${game_headers})
target_link_libraries(nomad_game_gdextension PRIVATE Nomad::Framework godot::cpp)
target_include_directories(nomad_game_gdextension PRIVATE Bootstrap Source)
target_compile_definitions(nomad_game_gdextension PRIVATE
	GDEXTENSION_ENTRY_SYMBOL=nomad_game_library_init
)
set_target_properties(nomad_game_gdextension PROPERTIES
	PREFIX ""
	OUTPUT_NAME "nomad_game_gdextension.${NOMAD_PLATFORM_NAME}.${GODOTCPP_TARGET}.x86_64"
	RUNTIME_OUTPUT_DIRECTORY "$<1:${CMAKE_CURRENT_SOURCE_DIR}/../addons/nomad/bin/${NOMAD_PLATFORM_NAME}>"
	LIBRARY_OUTPUT_DIRECTORY "$<1:${CMAKE_CURRENT_SOURCE_DIR}/../addons/nomad/bin/${NOMAD_PLATFORM_NAME}>"
	BUILD_RPATH "$ORIGIN"
	ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/lib/$<CONFIG>"
)
nomad_copy_runtime_libraries(nomad_game_gdextension)
