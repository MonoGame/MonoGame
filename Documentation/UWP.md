# Game class constructor
Due to some UWP implementation details, MonoGame has to construct your `Game` derived class by itself, using a static initializer `MonoGame.Framework.XamlGame<T>.Create(...)`.

In this situation, you have two main possibilities to create a `Game` derived class:

1. Let `XamlGame` initialize your `Game` derived class using the default constructor
2. Let `XamlGame` initialize your `Game` derived class using a custom constructor.

#### 1. XamlGame uses the default constructor

With this logic, it isn't possible to inject dependencies through the constructor since the default constructor is called:
 `var game = new T();`



#### 2. XamlGame uses a custom constructor

Why may you need this constructor?

Consider `Game1` needs some dependencies such as an `ISettingsRepository` to get some values from each *platform* settings store. You would then implement an `AndroidSettingsRepository` and a `UwpSettingsRepository`, but you cannot construct those dependencies in `Game1` itself, **because they are platform dependent**, so you'll have to inject them into its constructor.

For example, in a `MainActivity` on Android you would do:

```csharp
_game = new Game1(
    new AndroidTextFileImporter(Assets),
    new AndroidSettingsRepository(this));
```

With the UWP implementation using `XamlGame` static initializer, you could do this:

```csharp
_game = MonoGame.Framework.XamlGame<Game1>.Create(
	launchArguments,
	Window.Current.CoreWindow,
	swapChainPanel,
	() => new Game1(
		new UwpTextFileImporter(Assets),
		new UwpSettingsRepository(this)));
```

In this way, you tell the static initializer **how** you'd like to construct `Game1`.

