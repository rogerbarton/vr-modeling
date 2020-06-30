# C++ API Reference

.. note:: 

   This is aimed at people wanting to view or edit the code. 

The C++ code is found in `Interface/source` and is compiled to a .dll (shared library) with CMake. It is *automatically* copied into the `Assets/Plugins` folder so we can use it with Unity. Note that the C++ code is where all the geometry and performance intensive code resides. There is no `main()` function instead `Interface.h` includes all the exported functions that can be called from C#.

.. toctree::
   :maxdepth: 2
   :caption: Contents:

   cpp-interface
   source/index
   external/index