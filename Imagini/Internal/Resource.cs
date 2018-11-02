using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Imagini
{
    /// <summary>
    /// Represents a resource which can be disposed.
    /// </summary>
    public abstract class Resource
    {
        private List<Resource> _children = new List<Resource>();
        private readonly string _resourceName;

        internal Resource(string resourceName) =>
            _resourceName = resourceName;


        /// <summary>
        /// Indicates if this resource is disposed or not.
        /// </summary>
        public bool IsDisposed { get; private set; }
        internal virtual void Destroy()
        {
            if (IsDisposed) return;
            _children.ForEach(c => c.Destroy());
            _children = null;
            IsDisposed = true;
        }

        protected void CheckIfNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(_resourceName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T NotDisposed<T>(Func<T> val)
        {
            CheckIfNotDisposed();
            return val();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void NotDisposed(Action action)
        {
            CheckIfNotDisposed();
            action();
        }

        internal void Register(Resource child) => _children.Add(child);
    }
}