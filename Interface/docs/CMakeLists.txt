cmake_minimum_required (VERSION 3.8)
project("vr-modeling-docs")

set(CMAKE_MODULE_PATH "${PROJECT_SOURCE_DIR}/cmake" ${CMAKE_MODULE_PATH})

find_package(Doxygen REQUIRED)
find_package(Sphinx REQUIRED)
if(NOT DOXYGEN_FOUND OR NOT SPHINX_FOUND)
    message(STATUS "Skipping documentation generation, make sure your have installed doxygen and done 'pip install -r docs/requirements.txt'")
    return()
endif()

# --- Doxygen
# Find all source files for efficient recompile
set(CS_SCRIPTS_DIR "${UNITY_PROJECT_ROOT}Assets/Scripts")
set(CXX_SCRIPTS_DIR "${INTERFACE_PROJECT_ROOT}")
file(GLOB_RECURSE CS_SCRIPTS "${CS_SCRIPTS_DIR}/*.cs")
set(CXX_SCRIPTS ${HFILES} ${SRCFILES})

# Set input/output directories
set(DOXYGEN_INPUT_DIRECTORY "${CXX_SCRIPTS_DIR} ${CS_SCRIPTS_DIR}")
set(DOXYGEN_OUTPUT_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/doxygen)
set(DOXYFILE_IN ${CMAKE_CURRENT_SOURCE_DIR}/Doxyfile.in)
set(DOXYFILE_OUT ${CMAKE_CURRENT_BINARY_DIR}/Doxyfile)
# Replace variables inside @@ with the current values
configure_file(${DOXYFILE_IN} ${DOXYFILE_OUT} @ONLY)

# Create output directory (not done by doxygen)
file(MAKE_DIRECTORY ${DOXYGEN_OUTPUT_DIRECTORY})

set(DOXYGEN_INDEX_FILE ${CMAKE_CURRENT_SOURCE_DIR}/xml/index.xml)
add_custom_command(OUTPUT ${DOXYGEN_INDEX_FILE}
                   DEPENDS ${CS_SCRIPTS} ${CXX_SCRIPTS}
                   COMMAND ${DOXYGEN_EXECUTABLE} ${DOXYFILE_OUT}
                   WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
                   MAIN_DEPENDENCY Doxyfile.in
                   COMMENT "Generating doyxgen xml docs")

add_custom_target(Doxygen ALL DEPENDS ${DOXYGEN_INDEX_FILE})

# --- Sphinx

set(SPHINX_SOURCE ${CMAKE_CURRENT_SOURCE_DIR})
set(SPHINX_BUILD ${CMAKE_CURRENT_BINARY_DIR}/docs/sphinx)

add_custom_target(Sphinx ALL
                  COMMAND
                  ${SPHINX_EXECUTABLE} -b html
                  -Dbreathe_projects.vr-modeling=_doxygen/xml/
                  ${SPHINX_SOURCE} ${SPHINX_BUILD}
                  WORKING_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}
                  COMMENT "Generating documentation with Sphinx")