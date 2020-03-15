cmake_minimum_required(VERSION 3.1)
project(__libigl-interface)

# Add project files
file(GLOB SRCFILES "Interface.cpp" "*.cpp")
file(GLOB HFILES "Interface.h" "*.h")
source_group("Header Files" FILES ${HFILES})
if(CMAKE_VERBOSE)
	message(STATUS "Source Files ${PROJECT_NAME}: ${SRCFILES}")
	message(STATUS "Header Files ${PROJECT_NAME}: ${HFILES}")
endif()

# Create out DLL output file and link libigl to it
add_library(${PROJECT_NAME} SHARED ${SRCFILES} ${HFILES})
target_link_libraries(${PROJECT_NAME} igl::core)
target_include_directories(${PROJECT_NAME} PRIVATE .)

# Ensure the output dll is placed automatically into the Assets/Plugins folder (same path for release and debug)
set_target_properties(${PROJECT_NAME} PROPERTIES 
	RUNTIME_OUTPUT_DIRECTORY_DEBUG "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}"
	RUNTIME_OUTPUT_DIRECTORY_RELEASE "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")
message(STATUS "Saving ${PROJECT_NAME}.dll to: ${UNITY_DLL_OUTPUT_DIRECTORY_ABS}")

#set_property(DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR} PROPERTY VS_STARTUP_PROJECT ${PROJECT_NAME})\
