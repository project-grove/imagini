# MouseMoveEventArgs(int, int, int, int, MouseButtons, Window)

**Constructor**

**Namespace:** [Imagini](Imagini.md)

**Declared in:** [Imagini.MouseMoveEventArgs](Imagini.MouseMoveEventArgs.md)

------



Creates new event args object.


## Syntax

```csharp
public MouseMoveEventArgs(
	int x,
	int y,
	int relX,
	int relY,
	MouseButtons buttons,
	Window window
)
```

### Parameters

`x`

X coordinate relative to window.

`y`

Y coordinate relative to window.

`relX`

Relative motion in X direction.

`relY`

Relative motion in Y direction.

`buttons`

Mouse button state.

`window`

Target window. If null, the currently focused one is used.

------

[Back to index](index.md)