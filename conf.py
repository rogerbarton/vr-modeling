# -*- coding: utf-8 -*-
#
# vr-modeling documentation build configuration file, created by
# sphinx-quickstart on Fri Jun 19 17:39:11 2020.
#
# This file is execfile()d with the current directory set to its
# containing dir.
#
# Note that not all possible configuration values are present in this
# autogenerated file.
#
# All configuration values have a default; values that are commented out
# serve to show the default.

# If extensions (or modules to document with autodoc) are in another directory,
# add these directories to sys.path here. If the directory is relative to the
# documentation root, use os.path.abspath to make it absolute, like shown here.
#
# import os
# import sys
# sys.path.insert(0, os.path.abspath('.'))

import subprocess
import os
import sys

# --- Manual Configuration for ReadTheDocs server


def configure_doxyfile(input_dir, output_dir):
    with open('docs/Doxyfile.in', 'r') as file:
        filedata = file.read()

    filedata = filedata.replace('@DOXYGEN_INPUT_DIRECTORY@', input_dir)
    filedata = filedata.replace('@DOXYGEN_OUTPUT_DIRECTORY@', output_dir)

    with open('docs/Doxyfile', 'w') as file:
        file.write(filedata)


# Check if we're running on Read the Docs' servers
read_the_docs_build = os.environ.get('READTHEDOCS', None) == 'True'

breathe_projects = {}

if read_the_docs_build:
    input_dir = 'Interface/source \ Assets/Scripts \ Interface/external/Unity \ Interface/external/UnityNativeTool'
    output_dir = 'docs/doxygen'
    configure_doxyfile(input_dir, output_dir)
    subprocess.call('doxygen docs/Doxyfile', shell=True)
    breathe_projects['vr-modeling'] = output_dir + '/xml'
    print("---- Doxygen Done ----")
# --- End of manual configuration for ReadTheDocs server

# -- General configuration ------------------------------------------------

# If your documentation needs a minimal Sphinx version, state it here.
#
# needs_sphinx = '1.0'

# Add any Sphinx extension module names here, as strings. They can be
# extensions coming with Sphinx (named 'sphinx.ext.*') or your custom
# ones.

# Also look for extensions here
sys.path.append(os.path.abspath('docs/external'))

extensions = [ 'sphinx.ext.mathjax',
               'sphinx_csharp',
               'breathe',
               'm2r',
               'sphinxcontrib.bibtex',
               # 'open_in_newtab',
               ]

# Breathe
breathe_default_project = 'vr-modeling'
breathe_debug_trace_directives = False

# --- C# domain configuration ---
sphinx_csharp_test_links = read_the_docs_build
sphinx_csharp_multi_language = True

sphinx_csharp_ignore_xref = [
    'Vector2',
    'Vector3',
]

sphinx_csharp_ext_search_pages = {
    'upm.xrit': ('https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/api/%s.html',
                 'https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/?%s'),
    'upm.tmp': ('https://docs.unity3d.com/Packages/com.unity.textmeshpro@1.2/api/%s.html',
                'https://docs.unity3d.com/Packages/com.unity.textmeshpro@1.2/?%s'),
    'upm.ugui': ('https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/script-%s.html',
                 'https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/index.html?%s')
}

sphinx_csharp_ext_type_map = {
    'unity': {
        'XR': ['InputDevice', 'InputDeviceCharacteristics'],
        'Unity.Collections': ['NativeArray'],
        'Experimental.AssetImporters': ['AssetImportContext', 'MeshImportPostprocessor', 'ScriptedImporter'],
        'Rendering': ['VertexAttributeDescriptor'],
        'Events': ['UnityAction'],
    },
    'upm.xrit': {'UnityEngine.XR.Interaction.Toolkit': ['XRRayInteractor', 'XRBaseInteractable', 'XRController']},
    'upm.tmp': {'TMPro': ['TMP_Text']},
    'upm.ugui': {'': ['Image', 'Button', 'Toggle']},
}

sphinx_csharp_external_type_rename = {
    'NativeArray': 'NativeArray_1',
}

# Debug options
sphinx_csharp_debug = False
sphinx_csharp_debug_parse = False
sphinx_csharp_debug_parse_func = False
sphinx_csharp_debug_parse_var = False
sphinx_csharp_debug_parse_prop = False
sphinx_csharp_debug_parse_attr = False
sphinx_csharp_debug_parse_idxr = False
sphinx_csharp_debug_parse_type = False
sphinx_csharp_debug_xref = False
sphinx_csharp_debug_ext_links = False

# --- End of C# Domain ----

# Add any paths that contain templates here, relative to this directory.
templates_path = ['docs/_templates']

# The suffix(es) of source filenames.
# You can specify multiple suffix as a list of string:

source_suffix = ['.rst', '.md']

# The master toctree document.
master_doc = 'index'

# General information about the project.
project = u'vr-modeling'
copyright = u'2020, Roger Barton'
author = u'Roger Barton'

# The version info for the project you're documenting, acts as replacement for
# |version| and |release|, also used in various other places throughout the
# built documents.
#
# The short X.Y version.
version = u'1.0'
# The full version, including alpha/beta/rc tags.
release = u'1.0.0'

# The language for content autogenerated by Sphinx. Refer to documentation
# for a list of supported languages.
#
# This is also used if you do content translation via gettext catalogs.
# Usually you set "language" from the command line for these cases.
language = 'c++'
highlight_language = 'c++'

# List of patterns, relative to source directory, that match files and
# directories to ignore when looking for source files.
# This patterns also effect to html_static_path and html_extra_path
exclude_patterns = ['_build', 'Thumbs.db', '.DS_Store', 'Library/*', 'obj/*', 'Packages/*', 'cmake-build*',
                    'Interface/external/libigl/*', 'docs/external/*']

# The name of the Pygments (syntax highlighting) style to use.
pygments_style = 'sphinx'

# If true, `todo` and `todoList` produce output, else they produce nothing.
todo_include_todos = False


# -- Options for HTML output ----------------------------------------------

# The theme to use for HTML and HTML Help pages.  See the documentation for
# a list of builtin themes.
#
html_theme = 'sphinx_rtd_theme'

html_theme_options = {
    'canonical_url': 'https://vr-modeling.readthedocs.io/en/latest/',
    'analytics_id': '',  # Provided by Google in your dashboard
    'style_external_links': True,      # Show icon on external links
    'prev_next_buttons_location': 'both',

    # Toc options
    'collapse_navigation': True,
    'sticky_navigation': True,
    'navigation_depth': 4,
    'includehidden': True,
    'titles_only': False
}
html_logo = 'docs/_static/images/arap-usage.png'
github_url = 'https://github.com/rogerbarton/vr-modeling'
gitlab_url = 'https://gilab.ethz.ch/rbarton/vr-modeling'

# Add any paths that contain custom static files (such as style sheets) here,
# relative to this directory. They are copied after the builtin static files,
# so a file named "default.css" will overwrite the builtin "default.css".
html_static_path = ['docs/_static']
html_css_files = [
    'css/common.css'
]
html_js_files = [
    'js/custom.js'
]

# Prioritise gif's over png's in html
# Use myimage.* so the gif is shown in html but the png in the pdf
# See https://stackoverflow.com/questions/45969711/sphinx-doc-how-do-i-render-an-animated-gif-when-building-for-html-but-a-png-wh
from sphinx.builders.html import StandaloneHTMLBuilder
StandaloneHTMLBuilder.supported_image_types = [
    'image/svg+xml',
    'image/gif',
    'image/png',
    'image/jpeg'
]

# Custom sidebar templates, must be a dictionary that maps document names
# to template names.
#
# This is required for the alabaster theme
# refs: http://alabaster.readthedocs.io/en/latest/installation.html#sidebars
html_sidebars = {
    '**': [
        'relations.html',  # needs 'show_related': True theme option to display
        'searchbox.html',
    ]
}


# -- Options for HTMLHelp output ------------------------------------------

# Output file base name for HTML help builder.
htmlhelp_basename = 'vr-modelingdoc'


# -- Options for LaTeX output ---------------------------------------------

latex_elements = {
    # The paper size ('letterpaper' or 'a4paper').
    #
    # 'papersize': 'letterpaper',

    # The font size ('10pt', '11pt' or '12pt').
    #
    # 'pointsize': '10pt',

    # Additional stuff for the LaTeX preamble.
    #
    # 'preamble': '',

    # Latex figure (float) alignment
    #
    # 'figure_align': 'htbp',
}

# Grouping the document tree into LaTeX files. List of tuples
# (source start file, target name, title,
#  author, documentclass [howto, manual, or own class]).
latex_documents = [
    (master_doc, 'vr-modeling.tex', u'vr-modeling Documentation',
     u'Roger Barton', 'manual'),
]


# -- Options for manual page output ---------------------------------------

# One entry per manual page. List of tuples
# (source start file, name, description, authors, manual section).
man_pages = [
    (master_doc, 'vr-modeling', u'vr-modeling Documentation',
     [author], 1)
]


# -- Options for Texinfo output -------------------------------------------

# Grouping the document tree into Texinfo files. List of tuples
# (source start file, target name, title, author,
#  dir menu entry, description, category)
texinfo_documents = [
    (master_doc, 'vr-modeling', u'vr-modeling Documentation',
     author, 'vr-modeling', 'One line description of project.',
     'Miscellaneous'),
]



