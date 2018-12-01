![Stability: alpha](https://img.shields.io/badge/stability-alpha-orange.svg)

### Imagini - cross-platform .NET game/app framework
Imagini is a modular solution for building games and multimedia apps using .NET Core/Standard. It's based on SDL2 library and should run pretty much everywhere where the .NET runtime ands SDL2 library is present.

### Packages 
#### Imagini.Core
Base module. It implements the following functionality:
* **Window creation** with ability to choose backend
* **Event handling** - mouse, keyboard, gamepads, touch, window state and others
* **App/game loop** - variable and fixed timesteps, works similar to XNA/MonoGame loop
* **Display device enumeration** - queries available display devices and their modes

[API Documentation](https://project-grove.github.io/imagini/api/core/)

#### Imagini.2D
2D rendering module. Built around SDL2's [Render](https://wiki.libsdl.org/CategoryRender) and [Surface](https://wiki.libsdl.org/CategorySurface) APIs.

It provides the following features:
* **Drawing sprites** at any position and angle, from the whole texture or it's part
* Several **blend modes and modulation**
* Drawing single-pixel **points**, single-pixel **lines**, wireframe and filled **rectangles**
* **Render-to-texture** if the underlying backend supports it
* Texture and surface pixel data manipulation

It does not contain image file loaders.

[API Documentation](https://project-grove.github.io/imagini/api/2d/)


#### Imagini.ImageSharp
Texture and surface loader module. Allows for texture and surface creations from image files and streams. Powered by [ImageSharp](https://github.com/SixLabors/ImageSharp).

The following image file formats are supported:
* PNG
* JPEG
* BMP
* GIF

[API Documentation](https://project-grove.github.io/imagini/api/imagesharp/)

### Planned packages
* Imagini.Fonts - sprite font generation and drawing
* Imagini.Veldrid - [Veldrid](https://github.com/mellinoe/veldrid) integration

### Unimplemented features
#### Sound API
Check out the [vox](https://github.com/project-grove/vox) project. It provides a full 3D sound solution based on OpenAL.