# Virtual Reality in Unity

The main areas to consider when making this virtual reality compatible are:

1. **Rendering** - This is the part where Unity helps a lot, setting up all required things. This includes advanced features such as *single pass instanced* rendering, where we gain a lot performance.
1. **Input, Interaction & Locomotion (movement)** - The Unity XR Interaction Toolkit provides this
1. **UI** - We use the standard Unity UI with a *world space canvas*. Canvases are made grabbable so we can easily move them.
1. **Cross-platform** - By using only Unity packages we automatically have that this works for all suitable platforms. A consideration is that some VR devices have limited input (e.g. HTC Vive) and lack some key buttons/axes.

We also need to consider a *different way of interacting* with the world vs a mouse and keyboard. 

In particular, we have less degrees of freedom, in terms of the number of buttons, compared to a traditional keyboard and mouse. However, we can easily get absolute positional and rotational input. This indeed does allow for enough flexibility for this kind of application, but *the use of input combinations (or contextual input) is essential*. For example, when drawing a selection by pressing the trigger, the other buttons, should react differently to when not drawing. Currently, this is partially done and reflected in the input hints displayed on the controllers.

## Further Reading

- [Universal Render Pipeline (URP) v7.1.8](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@7.1/manual/index.html), the new render engine
- [XR Interaction Toolkit v0.9](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/manual/index.html)
- [Unity UI](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@7.1/manual/index.html), note see Text Mesh Pro (TMP) for text UI
