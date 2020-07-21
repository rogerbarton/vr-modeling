// SWIG File for generating the C# interface library
// Include windows specific keywords like __declspec
%include <windows.i>

%module Native
%{
// /* Includes the header in the wrapper code */
#include "Native.h"
%}

/* Parse the header file to generate wrappers */
%include "Native.h"