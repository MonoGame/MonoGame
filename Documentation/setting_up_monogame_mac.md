This section will help you setup MonoGame on Mac OSX.

### Running MonoGame Applications


### Developing MonoGame Applications

Developing on the Mac requires a number of other frameworks and applications. 
If you are targeting MacOS/iOS and/or Android you will need licences from [Xamarin](https://xamarin.com).
But to get started you can use the Linux or DesktopGL platforms which will run quite happily
on MacOS providing you have mono installed.

So to get setup you will first need to install mono. 
* Go to [Mono Downloads page] (http://www.mono-project.com/download/)
* Download the latest Mac OS installer.

Note: If you are running El Capitan you will need to install the very latest mono otherwise things 
will not work correctly.

You will also need Xamarin Studio
* Go to [xamarin.com](https://xamarin.com/download)
* Fill in the required informaton
* Download the installer.

This will install Xamarin Studio (which is Free) as well as other parts
of the Xamarin plafrom which you can optionally use. 

To setup MonoGame application development on mac OSX do the following:
* Go to [MonoGame Downloads page](http://www.monogame.net/downloads/)
* Click on the newest MonoGame release
* Download MonoGame for Mac
* Open the .pkg 
  * You will probably get an error about signing. If you do , right click and Open the .pkg file and you will be able to continue
* That's it, MonoGame is installed.

Make sure you install mono and Xamarin Studio first so that MonoGame can correctly setup the 
project templates and addins.
