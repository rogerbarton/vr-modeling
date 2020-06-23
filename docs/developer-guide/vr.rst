Virtual Reality
===============

The main areas to consider when making this virtual reality compatible are:

#. **Rendering** - This is the part where Unity helps a lot, setting up all required things. This includes advanced features such as single pass instanced rendering, where we gain of performance.
#. **Input, Interaction & Locomotion (movement)** - The Unity XR Interaction Toolkit provides this
#. **UI** - We use the standard Unity UI with a world space canvas. Canvases are made grabbable so we can easily move them.
#. **Cross-platform** - By using only Unity packages we automatically have that this works for all suitable platforms. A considerations is that some VR devices have limited input (e.g. HTC Vive).