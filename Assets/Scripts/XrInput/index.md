# C# XrInput

## InputManager.cs

The :cs:class:`InputManager` will detect and create the VR controllers. It handles getting of the current input state, i.e. which buttons are pressed, and saves this into the :cs:struct:`InputState`. The :cs:class:`InputManager` and the :cs:struct:`InputState` will also handle shared functionality, e.g. which tool is active, what the brush size is. 

.. doxygenclass:: XrInput::InputManager
   :members:
   :protected-members:
   :private-members:
   :undoc-members:

## InputState.cs

.. doxygenstruct:: XrInput::InputState
   :members:
   :protected-members:
   :private-members:
   :undoc-members:

### Enums

.. doxygenenum:: XrInput::ToolType

.. doxygenenum:: XrInput::ToolSelectMode

.. doxygenenum:: XrInput::ToolTransformMode

.. doxygenenum:: XrInput::SelectionMode

.. doxygenenum:: XrInput::PivotMode

## XrBrush.cs

.. doxygenclass:: XrInput::XrBrush
   :members:
   :protected-members:
   :private-members:
   :undoc-members:
