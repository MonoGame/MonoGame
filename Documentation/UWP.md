# Game class constructor
Due to some UWP implementation details, Monogame has to construct your `Game` derived class by itself, using a static initializer `MonoGame.Framework.XamlGame<T>.Create(...)`.

In this situation, we have to main possibilities to create a `Game` derived class:

1. Let `XamlGame` to initialize your `Game` derived class using the default (parameterless!) constructor
2. Let `XamlGame` to initialize your `Game` derived class using a custom (rich!) constructor.

##### 1. XamlGame uses the default parameterless constructor

With this logic, it isn't possible to inject in a "clear" way some dependencies, since the default constructor is called: `var game = new T();`



##### 2. XamlGame uses a custom rich constructor

Why may I need this constructor?

`MyGame` needs some dependencies such as an `ISettingsRepository` to get some values from each *platform* settings store. I would then implement an `AndroidSettingsRepository` and an `UwpSettingsRepository`. I cannot construct those dependencies in `MyGame` itself, **because they are platform dependent**, so I have to inject them into `MyGame` constructor.

For example, in a `MainActivity` on Android I would do:

```c#
_game = new MyGame(
    new AndroidTextFileImporter(Assets),
    new AndroidSettingsRepository(this));
```

With the UWP implementation using `XamlGame` static initializer, I could do this:

```c#
_game = MonoGame.Framework.XamlGame<MyGame>.Create(
	launchArguments,
	Window.Current.CoreWindow,
	swapChainPanel,
	() => new MyGame(
		new AndroidTextFileImporter(Assets),
		new AndroidSettingsRepository(this)));
```


