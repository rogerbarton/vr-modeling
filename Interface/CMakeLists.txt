cmake_minimum_required(VERSION 3.1)
project(libigl-interface)

# Add your project files
file(GLOB SRCFILES *.cpp)
file(GLOB HFILES *.h)


set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} ${CMAKE_CURRENT_SOURCE_DIR}/cmake)

# libigl - turn off 2D UI
option(LIBIGL_WITH_OPENGL            "Use OpenGL"         OFF)
option(LIBIGL_WITH_OPENGL_GLFW       "Use GLFW"           OFF)

find_package(LIBIGL REQUIRED QUIET)

add_library(${PROJECT_NAME} SHARED ${SRCFILES} ${HFILES})
target_link_libraries(${PROJECT_NAME} igl::core)
