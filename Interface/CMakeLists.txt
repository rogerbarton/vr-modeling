cmake_minimum_required(VERSION 3.1)
project(__libigl-interface)
set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} ${CMAKE_CURRENT_SOURCE_DIR}/cmake)
option(CMAKE_VERBOSE "Print extra information" OFF)

# Note: As this project is aimed at the Oculus Rift platform on Windows,
#       this cmake file is tailored to a Visual Studio solution.

# Add Unity header files
include_directories(${PROJECT_NAME} "external" "external/Unity")
file(GLOB UNITY_PLUGIN_API_FILES "external/Unity/PluginAPI/*.c*" "external/Unity/PluginAPI/*.h*")
file(GLOB EIGEN_DEBUG_FILES "external/eigen-debug/msvc/*")
option(UNITY_INCLUDE_RENDER_API "Include the Unity Render API Files in the project")
if(UNITY_INCLUDE_RENDER_API)
	file(GLOB UNITY_RENDER_API_FILES "external/Unity/RenderAPI/*.c*" "external/Unity/RenderAPI/*.h*" "external/Unity/RenderAPI/gl3w/*.c*" "external/Unity/RenderAPI/gl3w/*.h*")
else()
	set(UNITY_RENDER_API_FILES "")
endif()


# Find project files
file(GLOB SRCFILES "${SOURCE_DIR}/Native.cpp" "${SOURCE_DIR}/*.cpp")
file(GLOB HFILES   "${SOURCE_DIR}/Native.h"   "${SOURCE_DIR}/*.h")

# Add the dll projects
add_subdirectory("source")

# Uncomment to set our default build target in Visual Studio
# set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT ${PROJECT_NAME})


# Optionally build the UnityNativeTool stubLluiPlugin required, needs to only be built once usually
option(UNITY_BUILD_STUB_PLUGIN "Create a target for building the stubLluiPlugin library. 
	This has to be built once, in release mode, to allow for UnityPluginLoad and
	UnityPluginUnload to be called when mocking with UnityNativeTool in the Editor" ON)
if(${UNITY_BUILD_STUB_PLUGIN})
	add_subdirectory("external/UnityNativeTool") # stub plugin for ensuring UniyLoadPlugin is called when mocking
endif()
