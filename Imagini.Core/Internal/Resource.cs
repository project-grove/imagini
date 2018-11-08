using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Imagini.Logger;

namespace Imagini
{
    /// <summary>
    /// Represents a resource which can be disposed.
    /// </summary>
    public abstract class Resource
    {
        private List<Resource> _children = new List<Resource>();
        private readonly string _resourceName;

        private static Dictionary<Type, int> s_count = new Dictionary<Type, int>();
        internal int ResourceID { get; private set; }

        internal Resource()
        {
            var type = this.GetType();
            _resourceName = type.Name;
            if (!s_count.ContainsKey(type))
            {
                s_count.Add(type, 0);
            }
            ResourceID = s_count[type];
            s_count[type]++;
            Log.Debug("Created {name} ID {id}", _resourceName, ResourceID);
        }


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
            Log.Debug("Destroyed {name} ID {guid}", _resourceName, ResourceID);
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

        internal void Unregister(Resource child) => _children.Remove(child);
    }
}