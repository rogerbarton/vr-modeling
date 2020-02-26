cmake_minimum_required(VERSION 3.1)
project(libigl-interface)
set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} ${CMAKE_CURRENT_SOURCE_DIR}/cmake)

# Note: As this project is aimed at the Oculus Rift platform on Windows,
#       this cmake file is tailored to a Visual Studio solution.

# Add project files, for now just include all 
file(GLOB SRCFILES *.cpp)
file(GLOB HFILES *.{h, hpp})


# libigl - turn off 2D UI
option(LIBIGL_WITH_OPENGL            "Use OpenGL"         OFF)
option(LIBIGL_WITH_OPENGL_GLFW       "Use GLFW"           OFF)

find_package(LIBIGL REQUIRED QUIET)

# Create out DLL output file and link libigl to it
add_library(${PROJECT_NAME} SHARED ${SRCFILES} ${HFILES})
target_link_libraries(${PROJECT_NAME} igl::core)
set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT ${PROJECT_NAME})

# Ensure the output dll is placed automatically into the Assets/Plugins folder (same path for release and debug)
set(UNITY_PROJECT_ROOT "${CMAKE_CURRENT_LIST_DIR}/../" CACHE STRING 
	"Path to Unity project root where Assets and ProjectSettings resides")
set(UNITY_DLL_OUTPUT_DIRECTORY "Assets/Plugins/LibiglInterface" CACHE STRING 
	"Where the DLL will be after building, this should be inside the Assets/Plugins folder. Note: this should be and empty directory as everything is cleared on build")
set(UNITY_DLL_OUTPUT_DIRECTORY_ABS "${UNITY_PROJECT_ROOT}${UNITY_DLL_OUTPUT_DIRECTORY}")

set_target_properties(${PROJECT_NAME} PROPERTIES 
	RUNTIME_OUTPUT_DIRECTORY_DEBUG "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}"
	RUNTIME_OUTPUT_DIRECTORY_RELEASE "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")
message(STATUS "Saving ${PROJECT_NAME}.dll to: ${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")
