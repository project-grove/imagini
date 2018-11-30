# AppBase

**Class**

**Namespace:** [Imagini](Imagini.md)

**Base types:**

* [IDisposable](#.md)


**Declared in:** [Imagini](Imagini.md)

------



Base app class which instantiates a window and event loop.
Derive from this if you want to provide your own renderer for the
game loop.


## Members

### Property
* [Window](Imagini.WindowStateChangeEventArgs.Window.md)
* [Events](Imagini.Events.md)
* [TotalTime](Imagini.AppBase.TotalTime.md)
* [ElapsedAppTime](Imagini.AppBase.ElapsedAppTime.md)
* [IsMouseVisible](Imagini.AppBase.IsMouseVisible.md)
* [IsActive](Imagini.AppBase.IsActive.md)
* [TargetElapsedTime](Imagini.AppBase.TargetElapsedTime.md)
* [InactiveSleepTime](Imagini.AppBase.InactiveSleepTime.md)
* [MaxElapsedTime](Imagini.AppBase.MaxElapsedTime.md)
* [IsFixedTimeStep](Imagini.AppBase.IsFixedTimeStep.md)
* [IsRunningSlowly](Imagini.AppBase.IsRunningSlowly.md)
* [IsDisposed](Imagini.Resource.IsDisposed.md)

### Constructor
* [AppBase(WindowSettings)](Imagini.AppBase.AppBase(WindowSettings).md)

### Method
* [CancelExitRequest()](Imagini.AppBase.CancelExitRequest().md)
* [Dispose()](Imagini.AppBase.Dispose().md)
* [RequestExit()](Imagini.AppBase.RequestExit().md)
* [ResetElapsedTime()](Imagini.AppBase.ResetElapsedTime().md)
* [Run()](Imagini.AppBase.Run().md)
* [SuppressDraw()](Imagini.AppBase.SuppressDraw().md)
* [Terminate()](Imagini.AppBase.Terminate().md)
* [Tick()](Imagini.AppBase.Tick().md)

------

[Back to index](index.md)