# 3D Modeling in VR - Bachelor Thesis

## Setup

**Required Tools**: Unity, CMake, Visual Studio, 'Desktop development with C++' workload in the VS installer

**After Cloning:**
- Add the Oculus Integration to the project from the asset store
- (Optionally) add Odin Inspector to the project from the asset store
- Setup the C++ interface to libigl with CMake in the `Interface` folder
  1. Run cmake inside the Interface C++ project, this will enable building the `.dll` and copy it into the Unity project
  2. Note that the output directory can be set in the CMake cache with the `UNITY_*` variables
  3. This output directory should be empty as it is cleared on each build!
  
 ## Misc
 
 [*UnityNativeTool* from mcpiroman](https://github.com/mcpiroman/UnityNativeTool) is used to be able to rebuild a dll without restarting unity