# General Advice

## Further Reading

You should make yourself familiar with the Unity engine and some of its packages as well as libigl.

- [Valem VR YouTube Tutorials](https://youtu.be/gGYtahQjmWQ)
- [XR Interaction Toolkit Documentation](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/manual/index.html)
- [libigl Tutorial](https://libigl.github.io/tutorial/)

## How to start developing

1. Find a good IDE and learn how to do basic things such as:
   1. Go to definition (!)
   1. Refactor names
   1. Collapse all comments, #regions
   1. Syntax highlighting and intellisense
   1. Display quick documentation, `Crl` + `Q` in JetBrains
1. Setup your debugger so you can set a breakpoint
1. Set up/find out some keyboard shortcuts
1. In C++ use the :c:macro:`LOG` macros
1. Have a look at the existing functionality and use it as an example

## C# Features

- Use LINQ for manipulating arrays and lists
- Follow some of the advice from the [Refactoring Guru](https://refactoring.guru/)
  - Mainly, keep files small and separate independent features.

## Use of Bitmasks

Bitmasks are used often in this project for compact/efficient boolean storage. A common example is the selection vector. We have one 32-bit integer for each vertex, equivalent to 32 booleans per vertex. Each bit represents if the vertex is in that selection or not. There are some common operations with bitwise operators you may want to do. Please consider operator precedence.

1. Check if i-th bit is set `(flags & 1 << i) > 0`
1. Set i-th bit `flags |= 1 << i;`
1. Unset i-th bit `flags &= ~(1 << i);`

Note that `1 << i` can also be a mask of several bits or a predefined constant.

## VR Hands

- Anything that is related to the controllers always has a left L and right R. This means lots of parameters are essentially duplicated.
- Functions that are independent of these variables are often made and named `*Generic()`
- For booleans:  `Left = false`, `Right = true`

## Advanced

1. Be aware of the 'Enter Play Mode Settings' in `Project Settings > Editor`
   1. Reload Domain is expensive but ensures that things such as static variables are properly reset when pressing play. This can cause issues not present in a Build, make sure you clean up during OnDestroy.