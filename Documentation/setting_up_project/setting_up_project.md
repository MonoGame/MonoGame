# Setting up a new project

In this section, we shall go over some proper ways to set up a new project. MonoGame based projects usually require you to have a shared code and content base that can be used between platforms. As such, it's better to set everything up early on.

There are two common ways to approach this problem, and those are:

## 1. Using a PCL / .NET Standard library project

PCL library projects get compiled into a library that can be referenced by almost any other .NET project. All the platforms support them; however, they have been deprecated and superseded by .NET Standard library projects.

.NET Standard library projects have the same goal as PCL library projects, except they have higher target framework requirements. In case of using them with MonoGame, you would need to use at least .NET Standard 2.0, which requires .NET Framework 4.7.1 or .NET Core 2.0 as the deploy target.

### Positive:
- Great for writing libraries that you want to distribute
- You don't need to worry if the code would fail to compile on platform X because it has a different C# API

### Negative:
- The target framework you select might not work on some platforms, so you have to be careful
- You can't invoke any Native functions, but instead you will need to setup interfaces and assign them from outside the projct if you need to do so

## 2. Using a shared library project

Shared library projects do not get compiled into their own thing. Instead, all the files in them get automatically included during the compilation of the project that references it.

### Positive:
- You don't need to worry about .NET Framework version, but you do need to worry about C# version you are using
- You can easily hack stuff up for specific platforms without using interfaces or anything like that

### Negative:
- Currently, they are in a limbo-like state where some .NET tooling does not work with them, and there is no fix in sight
- Code might not compile on some platforms out of the box

## Conclusion

Now that you know the basic differences between the two, please select the tooling you plan on using, and we will get right into explaining a good way you can set it up:

- [Visual Studio Code](setting_up_project_vscode.md)
