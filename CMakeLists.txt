cmake_minimum_required(VERSION 3.1)
project(vr-modeling)
option(CMAKE_VERBOSE "Print extra information" OFF)

# Ensure the output dll is placed automatically into the Assets/Plugins folder (same path for release and debug)
set(UNITY_PROJECT_ROOT "${PROJECT_SOURCE_DIR}" CACHE STRING
	"Path to Unity project root where Assets and ProjectSettings resides")

set(UNITY_DLL_OUTPUT_DIRECTORY "Assets/Plugins" CACHE STRING 
	"Where the DLL will be after building, this should be inside the Assets/Plugins folder. 
	Note: this should be and empty directory as everything is cleared on build")

set(UNITY_DLL_OUTPUT_DIRECTORY_ABS "${UNITY_PROJECT_ROOT}/${UNITY_DLL_OUTPUT_DIRECTORY}")

# Add the dll projects
set(SOURCE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/Interface/source")
add_subdirectory("Interface")

# Documentation
add_subdirectory("docs")