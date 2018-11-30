# Poll(bool)

**Method**

**Namespace:** [Imagini](Imagini.md)

**Declared in:** [Imagini.EventManager](Imagini.EventManager.md)

------



Gathers all available events and distributes them to the
corresponding queues.


## Syntax

```csharp
public static unsafe void Poll(
	bool suppressGlobalProcessing
)
```

### Parameters

`suppressGlobalProcessing`


If true, global event queue will not be processed after calling this method.


------

[Back to index](index.md)