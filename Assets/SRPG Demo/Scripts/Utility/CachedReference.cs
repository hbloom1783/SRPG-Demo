using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SRPGDemo.Utility
{
    public class CachedReference<T> where T : class
    {
        public delegate T Getter();

        private Getter getter;

        public CachedReference(Getter getter = null)
        {
            this.getter = getter;
        }

        private T _cache = null;
        public T cache
        {
            get
            {
                if ((_cache == null) && (getter != null)) _cache = getter();
                return _cache;
            }
        }

        public static implicit operator T(CachedReference<T> subject)
        {
            return subject.cache;
        }

        public void Reset()
        {
            _cache = null;
        }
    }
}