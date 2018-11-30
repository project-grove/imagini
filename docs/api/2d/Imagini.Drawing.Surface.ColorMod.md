# ColorMod

**Property**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Surface](Imagini.Drawing.Surface.md)

------



Gets or sets the additional color value multiplied into blit operations.
Only R, G and B channels are used.


## Syntax

```csharp
public Color ColorMod { public get; public set; }
```

## Remarks

When this surface is blitted, during the blit operation each source color channel is modulated by the appropriate color value according to the following formula:
srcC = srcC * (color / 255)

------

[Back to index](index.md)