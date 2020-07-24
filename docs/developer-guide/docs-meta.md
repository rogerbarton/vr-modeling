# Documentation Meta

Documentation is generated with doxygen and then fed to sphinx via the breathe extension. A custom csharp domain (sphinx-csharp) is used to be able to render the C# code as well. By using the m2r extension markdown is converted to reStructuredText `.rst` which is what sphinx expects. This allows us to write markdown with inline rst. rst directives are used to access the doxygen documentation (see breathe).

Code is documented with xml-doc in C# and javadoc in C++ currently. Using `Crl` + `Q` in JetBrains IDEs allows displaying the documentation quickly inline when developing.

.. note::

   The markdown files are meant to give an overview, but most documentation should be in the code itself. The recommended cross-platform md/rst editor that I use is [Typora](https://typora.io/).

## Modifying the Documentation

- Annotate your code (e.g. functions, vars) with either xml-doc or javadoc
- You can add a new md file but it must be referenced in a `toctree` directive (usually found in `index.*`). CMake must re-run when doing this.
- Ideally have a markdown file per source directory

After making changes fast forward the `read-the-docs` branch to the latest commit on the master branch. This will trigger read the docs to rebuild and update the online documentation.