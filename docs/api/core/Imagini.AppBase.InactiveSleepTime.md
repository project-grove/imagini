# InactiveSleepTime

**Property**

**Namespace:** [Imagini](Imagini.md)

**Declared in:** [Imagini.AppBase](Imagini.AppBase.md)

------



Gets or sets the target time between each frame if the window is
inactive and [IsFixedTimeStep](Imagini.AppBase.IsFixedTimeStep.md) is set to true.


## Syntax

```csharp
public TimeSpan InactiveSleepTime { public get; public set; }
```

## Remarks
Default is ~33 ms (30 FPS).
------

[Back to index](index.md)