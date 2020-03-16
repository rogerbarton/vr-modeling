cmake_minimum_required(VERSION 3.1)
project(__libigl-interface)
set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} ${CMAKE_CURRENT_SOURCE_DIR}/cmake)
option(CMAKE_VERBOSE "Print extra information" OFF)

# Note: As this project is aimed at the Oculus Rift platform on Windows,
#       this cmake file is tailored to a Visual Studio solution.

# libigl - turn off 2D UI
option(LIBIGL_WITH_OPENGL            "Use OpenGL"         OFF)
option(LIBIGL_WITH_OPENGL_GLFW       "Use GLFW"           OFF)
find_package(LIBIGL REQUIRED QUIET)
find_package(OpenGL REQUIRED)

# Add Unity header files
include_directories(${PROJECT_NAME} "external")
file(GLOB UNITY_API_FILES "external/Unity/*.h*" "external/Unity/*.c*" "external/Unity/gl3w/*.h*" "external/Unity/gl3w/*.c*")
file(GLOB EIGEN_DEBUG_FILES "external/eigen-debug/msvc/*")


# Ensure the output dll is placed automatically into the Assets/Plugins folder (same path for release and debug)
set(UNITY_PROJECT_ROOT "${PROJECT_SOURCE_DIR}/../" CACHE STRING 
	"Path to Unity project root where Assets and ProjectSettings resides")

set(UNITY_DLL_OUTPUT_DIRECTORY "Assets/Plugins" CACHE STRING 
	"Where the DLL will be after building, this should be inside the Assets/Plugins folder. 
	Note: this should be and empty directory as everything is cleared on build")

set(UNITY_DLL_OUTPUT_DIRECTORY_ABS "${UNITY_PROJECT_ROOT}${UNITY_DLL_OUTPUT_DIRECTORY}")


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
