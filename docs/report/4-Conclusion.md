# Conclusion

A documented and working VR editor for libigl has been produced with plans for expansibility for adding more libigl features. It provides multi-mesh support as well as standard ways of editing a mesh with multiple vertex selections. This enables the interactive use of biharmonic and As-Rigid-As-Possible deformations. By providing visual aids via tooltips and input hints, user-friendliness is improved. 

The current 2D libigl editor still has a much larger feature set and there is a large barrier for converting applications to the VR viewer. As a result, the current application appears to be more suitable for demos. However, there is potential for this to see wider use cases in the future, if developed further. 

Using Unity as a basis is not optimal but has several advantages, particularly because of its ease of use and flatter learning curve. Workarounds have been found for the immediate shortcomings with the mesh interface, API thread-safety limitation and C++ interface. The discussed alternatives should be considered first before continuing this project.

.. bibliography:: references.bib
   :filter: docname in docnames
   :style: unsrt
   :labelprefix: 4.