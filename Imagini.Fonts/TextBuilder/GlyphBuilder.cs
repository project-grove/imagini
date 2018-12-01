﻿using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace SixLabors.Shapes.Temp
{
    /// <summary>
    /// rendering surface that Fonts can use to generate Shapes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class GlyphBuilder : BaseGlyphBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlyphBuilder"/> class.
        /// </summary>
        public GlyphBuilder()
            : this(Vector2.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlyphBuilder"/> class.
        /// </summary>
        /// <param name="origin">The origin.</param>
        public GlyphBuilder(Vector2 origin)
            : base()
        {
            this.builder.SetOrigin(origin);
        }
    }
}
