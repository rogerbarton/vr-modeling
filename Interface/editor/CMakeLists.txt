cmake_minimum_required(VERSION 3.1)
project(libigl-editor)

# Add project files
file(GLOB SRCFILES "IOInterface.cpp" "*.cpp")
file(GLOB HFILES "IOInterface.h" "*.h")
source_group("Header Files" FILES ${HFILES})

# Create out DLL output file and link libigl to it
add_library(${PROJECT_NAME} SHARED ${SRCFILES} ${HFILES})
target_link_libraries(${PROJECT_NAME} igl::core)
target_include_directories(${PROJECT_NAME} PRIVATE .)


set_target_properties(${PROJECT_NAME} PROPERTIES 
	RUNTIME_OUTPUT_DIRECTORY_DEBUG "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}"
	RUNTIME_OUTPUT_DIRECTORY_RELEASE "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")
message(STATUS "Saving ${PROJECT_NAME}.dll to: ${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")
