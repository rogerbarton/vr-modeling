# Virtual Reality

The main areas to consider when making this virtual reality compatible are:

1. **Rendering** - This is the part where Unity helps a lot, setting up all required things. This includes advanced features such as single pass instanced rendering, where we gain of performance.
1. **Input, Interaction & Locomotion (movement)** - The Unity XR Interaction Toolkit provides this
1. **UI** - We use the standard Unity UI with a world space canvas. Canvases are made grabbable so we can easily move them.
1. **Cross-platform** - By using only Unity packages we automatically have that this works for all suitable platforms. A considerations is that some VR devices have limited input (e.g. HTC Vive).

We also need to consider a different way of interacting with the world. In particular, we have less degrees of freedom, in terms of the number of buttons, compared to a traditional keyboard and mouse. However, we can easily get absolute positional and rotational input. This indeed does allow for enough flexibility for this kind of application, but the use of input combinations (or contextual input) is essential. For example, when drawing a selection by pressing the trigger, the other buttons, if pressed, should react differently to when not drawing. Here this is partially done and reflected in the input hints displayed on the controllers.