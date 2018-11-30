# BlendMode

**Enum**

**Namespace:** [Imagini.Drawing](Imagini.Drawing.md)

**Declared in:** [Imagini.Drawing](Imagini.Drawing.md)

------



Defines surface blend modes.


### Enumeration
**Underlying type:** int
Values:
* None = SDL_BlendMode.SDL_BLENDMODE_NONE
* AlphaBlend = SDL_BlendMode.SDL_BLENDMODE_BLEND
* Add = SDL_BlendMode.SDL_BLENDMODE_ADD
* Modulate = SDL_BlendMode.SDL_BLENDMODE_MOD



## Remarks

None:

```csharp

dstRGBA = srcRGBA

```

AlphaBlend:

```csharp

dstRGB = (srcRGB * srcA) + (dstRGB * (1 - srcA))
dstA = srcA + (dstA * (1 - srcA))

```

Add:

```csharp

dstRGB = (srcRGB * srcA) + dstRGB
dstA = dstA

```

Modulate:

```csharp

dstRGB = srcRGB * dstRGB
dstA = dstA

```

------

[Back to index](index.md)