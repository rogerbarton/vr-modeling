cmake_minimum_required(VERSION 3.1)
project(stubLluiPlugin)

set(CMAKE_BUILD_TYPE "Release")
add_library(${PROJECT_NAME} SHARED "stubLluiPlugin")
target_include_directories(${PROJECT_NAME} PRIVATE "../Unity")
set_target_properties(${PROJECT_NAME} PROPERTIES EXCLUDE_FROM_ALL TRUE) # Don't always build this library

set_target_properties(${PROJECT_NAME} PROPERTIES 
	RUNTIME_OUTPUT_DIRECTORY_DEBUG "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}/UnityNativeTool"
	RUNTIME_OUTPUT_DIRECTORY_RELEASE "${UNITY_DLL_OUTPUT_DIRECTORY_ABS}/UnityNativeTool")
message(STATUS "Saving ${PROJECT_NAME}.dll to: ${UNITY_DLL_OUTPUT_DIRECTORY_ABS}/UnityNativeTool")