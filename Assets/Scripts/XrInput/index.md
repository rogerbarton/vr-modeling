# C# XrInput

## InputManager.cs

The `InputManager` will detect and create the VR controllers. It handles getting of the current input state, i.e. which buttons are pressed, and saves this into the `InputState`. The `InputManager` and the `InputState` will also handle shared functionality, e.g. which tool is active, what the brush size is.

.. doxygenfile:: InputManager.cs

## InputState.cs

This is the 'shared' input state. It is also where you can access the filtered controller input.

.. doxygenfile:: InputState.cs

## XrBrush.cs

.. doxygenfile:: XrBrush.cs