THIS IS A WORK IN PROGRESS!

# MonoGame Documentation
This is the source for the [documentation published on MonoGame.net](http://www.monogame.net/documentation/).  It is rebuilt when the code changes and is published nightly to the website.

## General Rules
The following rules must be observed at all times when contributing documentation to the MonoGame project.

 - Write in a neutral, technical tone.
 - Avoid humor, personal opinions, and colloquial language.
 - **Never** plagiarize any documentation from another source.
 - Do not use automatic documentation tools as they are ineffective. 

Breaking these rules can result in your contribution being rejected.

## Getting Started
You can create and edit documentation right from the web browser without needing to install Git or ever leave the GitHub site.

 - [Fork the MonoGame repo](https://help.github.com/articles/fork-a-repo/).
 - [Create a new branch](https://help.github.com/articles/creating-and-deleting-branches-within-your-repository/) from `develop` and make your changes only in that branch.
 - [Create a new file](https://help.github.com/articles/creating-new-files/) or [edit an existing one](https://help.github.com/articles/editing-files-in-your-repository/) using the GitHub markup editor.
 - [Submit pull requests](https://help.github.com/articles/creating-a-pull-request/) early and often to merge your documentation changes.

## Style Guide
Review the following expectations before contributing any documentation.

### Manuals, Guides, and Tutorials
TODO!

### API Reference 
The API reference documentation is a big part of the documentation effort for MonoGame.  The documentation is written in the [C# XML format](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/xml-documentation-comments) and is inline to the MonoGame source code. The final web pages with API documentation are generated using [SharpDoc](https://github.com/xoofx/SharpDoc).

#### Every Word Should Contain Value
Every word in the reference documentation should provide information beyond the API itself.  Documentation that only rehashes or rephrases what is already apparent in the class, method, parameter, or property name has zero value and wastes time for both the writer and reader.

#### The First Sentence Is The Most Important
There is no guarantee that the reader will read beyond the first sentence of the reference documentation.  This is why that first sentence is the most important and should convey the most key piece of information.  Take your time to write the most concise and clear first sentence possible.  This helps users tremendously and goes a long way towards having great documentation.

#### Surface Information Hidden In the Code
Being inline with the code allows you to easily look for critical information within it that the user might not know from looking at the API alone.  Take your time to explore inner method calls and platform specific sections of the code.  The time to write the documentation is once you feel you fully understand the code you are documenting.  If you don't feel you understand the code then leave the documentation for someone else to write.

#### Documentation Is Referenced Not Read
Remember that the user is searching for an answer for a specific question.  It is your job to predict these questions and provide them clear answers.


## License
All documentation contributed to the MonoGame project is subject to the [Creative Commons Attribution-NonCommercial-ShareAlike](http://creativecommons.org/licenses/by-nc-sa/4.0/) license.  By contributing you are agreeing to the terms of that license.

<p align="center"><a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/"><img alt="Creative Commons License" style="border-width:0" src="http://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png" /></a><br /><span xmlns:dct="http://purl.org/dc/terms/" href="http://purl.org/dc/dcmitype/Text" property="dct:title" rel="dct:type">MonoGame Documentation</span> by the <a xmlns:cc="http://creativecommons.org/ns#" href="http://www.monogame.net" property="cc:attributionName" rel="cc:attributionURL">MonoGame Team</a> is licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/">Creative Commons Attribution-NonCommercial-ShareAlike License</a>.</p>
