The Common directory contains classes and XAML styles that simplify application development.

These are not merely convenient, but are required by most Visual Studio project and item templates.
If you need a variation on one of the styles in StandardStyles it is recommended that you make a
copy in your own resource dictionary.  When right-clicking on a styled control in the design
surface the context menu includes an option to Edit a Copy to simplify this process.

Classes in the Common directory form part of your project and may be further enhanced to meet your
needs.  Care should be taken when altering existing methods and properties as incompatible changes
will require corresponding changes to code included in a variety of Visual Studio templates.  For
example, additional pages added to your project are written assuming that the original methods and
properties in Common classes are still present and that the names of the types have not changed.