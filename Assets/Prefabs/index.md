# Prefabs

.. note::
	Please make sure you are familiar with Unity prefabs and prefab variants, as well as their icons/colors in the hierarchy.

## UI

This contains mostly instances of the UI components used when generating the UI panels.

**XrCanvas** is the 'base' prefab upon which all world space canvases are based on. This contains functionality on how to raycast the UI, how to grab the panels.

**MeshDetailsCanvas** is an empty details panel for a mesh, before any components have been added via C#.

**VerticalScrollList** defines the layout of generated elements. Elements are added to the `Content` child.

**InputHints** is the 'base' prefab for the hints shown on a controller. It is based on the Oculus Touch left hand controller and is used in the VR Controller prefabs.

**InputLabel** is one label used in the InputHints. Links to one button or axis. It is made so that it fits the content.

## UI Generator Components

These are items that can be added to the **VerticalScrollList**, in the **XrCanvas**, from C#. These are all referenced by the instance of the :cs:class:`UiManager` in the scene (found on the UI gameObject) under the `UI Component Prefabs` header.

## UI Input Hint Data

These aren't technically prefabs but :cs:class:`ScriptableObject` s, which is a simple data storage (compared to a database). This is where we store what the input hints will display. It is done in a hierarchical fashion with one instance per state and sub-state. This is used by :cs:func:`UiInputHints.Repaint`. See also :cs:func:`UiInputLabel.SetData`

## VR Controllers

This is where you can customize the look of the VR controllers, as well as tweaking the positions of the UI input hints for example.

## XR Interaction

These are prefabs related to the interaction with the world.