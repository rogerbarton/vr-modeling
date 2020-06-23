cmake_minimum_required (VERSION 3.8)
#project("vr-modeling-docs")
set(CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake" ${CMAKE_MODULE_PATH})

# This generates our documentation.
# First we run Doxygen to generate xml based understanding of the structure of our code
# Then we run python-based Sphinx to convert the .rst files to html
# Breathe is a Sphinx extension used to connect Doxygen and Sphinx
# See tutorial used: https://devblogs.microsoft.com/cppblog/clear-functional-c-documentation-with-sphinx-breathe-doxygen-cmake/

find_package(Doxygen)
find_package(Sphinx)
if(NOT DOXYGEN_FOUND OR NOT SPHINX_FOUND)
    message(STATUS "Skipping documentation generation, make sure your have installed doxygen and done 'pip install -r docs/requirements.txt'")
    return()
endif()

# --- Doxygen
# Find all source files for efficient recompilation
set(CS_FILES_DIR "${UNITY_PROJECT_ROOT}Assets/Scripts")
set(CXX_FILES_DIR "${SOURCE_DIR}")
set(UNITY_HFILES_DIR "${PROJECT_SOURCE_DIR}/external/Unity")
set(STUB_PLUGIN_DIR "${PROJECT_SOURCE_DIR}/external/UnityNativeTool")
file(GLOB_RECURSE CS_FILES "${CS_FILES_DIR}/*.cs")
set(CXX_FILES ${HFILES} ${SRCFILES})
if(CMAKE_VERBOSE)
    message(STATUS "Doc C#  files: ${CS_FILES}")
    message(STATUS "Doc C++ files: ${CXX_FILES}")
endif()


# Set input/output directories
set(DOXYGEN_INPUT_DIRECTORY "${CXX_FILES_DIR} \\ \n${CS_FILES_DIR} \\ \n${STUB_PLUGIN_DIR} \\ \n${UNITY_HFILES_DIR}")
set(DOXYGEN_OUTPUT_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/doxygen)

# Replace variables inside @@ with the current values
set(DOXYFILE_IN ${CMAKE_CURRENT_SOURCE_DIR}/Doxyfile.in)
set(DOXYFILE ${CMAKE_CURRENT_BINARY_DIR}/Doxyfile)
configure_file(${DOXYFILE_IN} ${DOXYFILE} @ONLY)
if(CMAKE_VERBOSE)
    message(STATUS "Using Doxyfile.in: ${DOXYFILE_IN}")
    message(STATUS "Doxygen outout at: ${DOXYGEN_OUTPUT_DIRECTORY}")
endif()

# Create output directory (not done by doxygen)
file(MAKE_DIRECTORY ${DOXYGEN_OUTPUT_DIRECTORY})

set(DOXYGEN_INDEX_FILE ${DOXYGEN_OUTPUT_DIRECTORY}/xml/index.xml)
add_custom_command(OUTPUT ${DOXYGEN_INDEX_FILE}
                   COMMAND ${DOXYGEN_EXECUTABLE} ${DOXYFILE}
                   DEPENDS ${DOXYFILE_IN} ${CS_FILES} ${CXX_FILES}
                   WORKING_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}
                   COMMENT "Generating Doyxgen xml documentation")

# This is the target we can execute                   
# Implementation note: we cannot use add_custom_target directly but need to defer it to the
#    DOXYGEN_INDEX_FILE, so that we dont always recompile
add_custom_target(Doxygen DEPENDS ${DOXYGEN_INDEX_FILE})


# --- Sphinx
set(SPHINX_SOURCE ${CMAKE_CURRENT_SOURCE_DIR})
set(SPHINX_BUILD ${CMAKE_CURRENT_BINARY_DIR}/sphinx)
file(GLOB_RECURSE DOC_RST_FILES ${UNITY_PROJECT_ROOT}/*.rst)
file(GLOB_RECURSE DOC_MD_FILES ${UNITY_PROJECT_ROOT}/*.md)
file(GLOB_RECURSE STATIC_FILES ${SPHINX_SOURCE}/_static/*)


if(CMAKE_VERBOSE)
    message(STATUS "rst files: ${DOC_RST_FILES}")
    message(STATUS "md  files: ${DOC_MD_FILES}")
    message(STATUS "static files: ${STATIC_FILES}")
endif()

add_custom_target(Sphinx
                  COMMAND
                    ${SPHINX_EXECUTABLE} -b html
                    # Tell breathe where to find the doxygen output
                    -Dbreathe_projects.vr-modeling="${DOXYGEN_OUTPUT_DIRECTORY}/xml"
                    ${SPHINX_SOURCE} ${SPHINX_BUILD}
                    
                  DEPENDS Doxygen "${SPHINX_SOURCE}/conf.py" ${DOC_RST_FILES} ${DOC_MD_FILES} ${STATIC_FILES}
                  WORKING_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}
                  COMMENT "Generating documentation with Sphinx")