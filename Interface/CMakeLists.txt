cmake_minimum_required(VERSION 3.1)
project(Interface)
set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} ${CMAKE_CURRENT_SOURCE_DIR}/cmake)

# Note: As this project is aimed at the Oculus Rift platform on Windows,
#       this cmake file is tailored to a Visual Studio solution.

# libigl - turn off 2D UI
option(LIBIGL_WITH_OPENGL            "Use OpenGL"         OFF)
option(LIBIGL_WITH_OPENGL_GLFW       "Use GLFW"           OFF)
find_package(LIBIGL REQUIRED QUIET)

#Add Unity header files
include_directories(${PROJECT_NAME} "external/PluginAPI")

# Ensure the output dll is placed automatically into the Assets/Plugins folder (same path for release and debug)
set(UNITY_PROJECT_ROOT "${PROJECT_SOURCE_DIR}/../" CACHE STRING 
	"Path to Unity project root where Assets and ProjectSettings resides")

set(UNITY_DLL_OUTPUT_DIRECTORY "Assets/Plugins" CACHE STRING 
	"Where the DLL will be after building, this should be inside the Assets/Plugins folder. 
	Note: this should be and empty directory as everything is cleared on build")

set(UNITY_DLL_OUTPUT_DIRECTORY_ABS "${UNITY_PROJECT_ROOT}${UNITY_DLL_OUTPUT_DIRECTORY}")

# Add two dll projects
add_subdirectory("source")
add_subdirectory("source/editor")
