# AlphaMod

**Property**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing.Surface](Imagini.Drawing.Surface.md)

------



Gets or sets an additional alpha value used in blit operations.


## Syntax

```csharp
public byte AlphaMod { public get; public set; }
```

## Remarks

When this surface is blitted, during the blit operation the source alpha value is modulated by this alpha value according to the following formula:
srcA = srcA * (alpha / 255)

------

[Back to index](index.md)