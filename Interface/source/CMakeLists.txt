cmake_minimum_required(VERSION 3.1)

source_group("Header Files" FILES ${HFILES})
source_group("Debug Misc" FILES ${EIGEN_DEBUG_FILES})
source_group("Unity\\Plugin API" FILES ${UNITY_PLUGIN_API_FILES})
if(UNITY_INCLUDE_RENDER_API)
	source_group("Unity\\Render API" FILES ${UNITY_RENDER_API_FILES})
endif()

if(CMAKE_VERBOSE)
	message(STATUS "Source Files ${PROJECT_NAME}: ${SRCFILES}")
	message(STATUS "Header Files ${PROJECT_NAME}: ${HFILES}")
	message(STATUS "Eigen Debug Files ${PROJECT_NAME}: ${EIGEN_DEBUG_FILES}")
	message(STATUS "Unity/Plugin API Files ${PROJECT_NAME}: ${UNITY_PLUGIN_API_FILES}")
	message(STATUS "Unity/Render API Files ${PROJECT_NAME}: ${UNITY_RENDER_API_FILES}")
endif()

# Create out DLL output file and link libigl to it
add_library(${PROJECT_NAME} SHARED ${SRCFILES} ${HFILES} ${UNITY_PLUGIN_API_FILES} ${UNITY_RENDER_API_FILES} ${EIGEN_DEBUG_FILES})

# libigl - turn off 2D UI
option(LIBIGL_WITH_OPENGL            "Use OpenGL"         OFF)
option(LIBIGL_WITH_OPENGL_GLFW       "Use GLFW"           OFF)
find_package(LIBIGL REQUIRED QUIET)
find_package(OpenGL REQUIRED)
target_link_libraries(${PROJECT_NAME} igl::core OpenGL::GL)
target_include_directories(${PROJECT_NAME} PRIVATE .)


# Ensure the output dll is placed automatically into the Assets/Plugins folder (same path for release and debug)
set_target_properties(${PROJECT_NAME} PROPERTIES 
	RUNTIME_OUTPUT_DIRECTORY_DEBUG "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}"
	RUNTIME_OUTPUT_DIRECTORY_RELEASE "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")
message(STATUS "Saving ${PROJECT_NAME}.dll to: ${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")
