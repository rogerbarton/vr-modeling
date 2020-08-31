# Documentation Meta

Documentation is generated with [Doxygen](https://www.doxygen.nl) and then fed to [Sphinx](https://www.sphinx-doc.org) via the [Breathe](https://github.com/michaeljones/breathe) extension. A custom C# domain [sphinx-csharp](https://github.com/djungelorm/sphinx-csharp) is used to be able to render the C# code as well. By using the [m2r](https://github.com/miyakogi/m2r) sphinx extension markdown is converted to reStructuredText `.rst` which is what sphinx expects. This allows us to write markdown with inline rst. rst directives are used to access the Doxygen documentation (see breathe).

Code is documented with xml-doc in C# and javadoc in C++ currently. Using `Crl` + `Q` in JetBrains IDEs allows displaying the documentation quickly inline when developing.

.. note::

   The markdown files are meant to give an overview, but most documentation should be in the code itself. The recommended cross-platform md/rst editor that I use is Typora, see https://typora.io

## Modifying the Documentation

- Annotate your code (e.g. functions, vars) with either xml-doc or javadoc
- You can add a new md file but it must be referenced in a `toctree` directive (usually found in `index.*`). CMake must re-run when doing this.
- Ideally have a markdown file per source directory
- Inline diagrams/flowcharts are made with [diagrams.net](https://diagrams.net) (previously draw.io) and inserted as an iframe
  - These are stored on [Google Drive](https://drive.google.com/drive/folders/1lwwUkIiIJvbDz0_dtgBPaxPXdL4pakT1?usp=sharing)
- Gifs in the gallery are created with [ScreenToGif](https://www.screentogif.com/)

After making changes fast forward the `read-the-docs` branch to the latest commit on the master branch. This will trigger read the docs to rebuild and update the online documentation.