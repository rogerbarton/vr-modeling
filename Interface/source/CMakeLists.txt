cmake_minimum_required(VERSION 3.1)

# -- Setup source files and logging
# Source groups that show up in Visual Studio solution explorer
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


# -- Create out DLL output file and link libigl to it
add_library(${PROJECT_NAME} SHARED ${SRCFILES} ${HFILES}
		${UNITY_PLUGIN_API_FILES} ${UNITY_RENDER_API_FILES} ${EIGEN_DEBUG_FILES})

# Ensure the output dll is placed automatically into the Assets/Plugins folder (same path for release and debug)
set_target_properties(${PROJECT_NAME} PROPERTIES
		RUNTIME_OUTPUT_DIRECTORY_DEBUG "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}"
		RUNTIME_OUTPUT_DIRECTORY_RELEASE "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")
message(STATUS "Saving ${PROJECT_NAME}.dll to: ${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")


# -- Find OpenMP for eigen parallel
find_package(OpenMP)
if(OpenMP_CXX_FOUND)
	message(STATUS "Eigen parallel enabled with OpenMP.")
	target_link_libraries(${PROJECT_NAME} OpenMP::OpenMP_CXX)
else()
	message(STATUS "Eigen parallel disabled, requires with OpenMP.")
endif()


# -- libigl - turn off 2D UI
# Note: Lock this section so that parallel CMake runs won't clash on eigen git checkout.
#       GUARD ensures this is released at the end of the scope, so find libigl last.
#       See: https://youtrack.jetbrains.com/issue/CPP-20496#focus=Comments-27-4273412.0-0
if(CMAKE_VERBOSE)
	message(STATUS "Acquiring libigl directory lock (for parallel cmake configures)")
endif()
file(LOCK "${EXTERNAL_DIR}/libigl" DIRECTORY GUARD FILE)

option(LIBIGL_WITH_OPENGL      "Use OpenGL" OFF)
option(LIBIGL_WITH_OPENGL_GLFW "Use GLFW"   OFF)

find_package(LIBIGL REQUIRED QUIET)
target_link_libraries(${PROJECT_NAME} igl::core)
target_include_directories(${PROJECT_NAME} PRIVATE .)
message(STATUS "Using libigl at: ${LIBIGL_INCLUDE_DIR}")