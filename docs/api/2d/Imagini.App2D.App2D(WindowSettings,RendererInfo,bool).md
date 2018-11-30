# App2D(WindowSettings, RendererInfo, bool)

**Constructor**

**Namespace:** [Imagini](Imagini.md)

**Declared in:** [Imagini.App2D](Imagini.App2D.md)

------



Creates a new app with the specified window settings.


## Syntax

```csharp
public App2D(
	WindowSettings settings,
	RendererInfo driver,
	bool useSurfaceApi
)
```

### Parameters

`driver`

Specifies a renderer to be used. If null, first hardware-accelerated renderer is used.

`useSurfaceApi`

If true, initializes [Surface](Imagini.Drawing.Surface.md) instead of [Graphics](Imagini.Drawing.Graphics.md)

## Remarks

If you have your own constructor, make sure to call this
one because it initializes the window and the event queue.

------

[Back to index](index.md)