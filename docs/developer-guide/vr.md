# Virtual Reality

The main areas to consider when making this virtual reality compatible are:

1. **Rendering** - This is the part where Unity helps a lot, setting up all required things. This includes advanced features such as single pass instanced rendering, where we gain of performance.
1. **Input, Interaction & Locomotion (movement)** - The Unity XR Interaction Toolkit provides this
1. **UI** - We use the standard Unity UI with a world space canvas. Canvases are made grabbable so we can easily move them.
1. **Cross-platform** - By using only Unity packages we automatically have that this works for all suitable platforms. A considerations is that some VR devices have limited input (e.g. HTC Vive).