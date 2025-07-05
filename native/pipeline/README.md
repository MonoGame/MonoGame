# MonoGame Native Pipeline Library

This folder contains the source code for the new native (C++) pipeline library for MonoGame Content Pipeline. This library will expose a C API for texture importing and processing, starting with bitmap import using stb_image.

## Structure
- All C++ source code for the native pipeline will live here.
- The library will be built as `mgpipeline` (dll/so/dylib).
- External dependencies (e.g., stb) will be referenced via submodules in `/external/stb`.
