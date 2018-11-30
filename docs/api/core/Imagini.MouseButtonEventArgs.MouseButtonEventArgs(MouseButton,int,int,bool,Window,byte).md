# MouseButtonEventArgs(MouseButton, int, int, bool, Window, byte)

**Constructor**

**Namespace:** [Imagini](Imagini.md)

**Declared in:** [Imagini.MouseButtonEventArgs](Imagini.MouseButtonEventArgs.md)

------



Creates a new event args object.


## Syntax

```csharp
public MouseButtonEventArgs(
	MouseButton button,
	int x,
	int y,
	bool isPressed,
	Window window,
	byte clicks
)
```

### Parameters

`button`

The button that changed.

`x`

X coordinate, relative to window.

`y`

Y coordinate, relative to window.

`isPressed`

Indicates if the button was pressed or released.

`window`

Target window. If null, the currently focused one is used.

`clicks`

Number of clicks.

------

[Back to index](index.md)