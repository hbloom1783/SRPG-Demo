using System.Collections.Generic;

namespace SRPGDemo.Utility
{
    public interface Poolable
    {
        void ResetForPool();
    }

    class ObjectPool<T> where T : Poolable, new()
    {
        private Queue<T> pooledObjects = new Queue<T>();

        public ObjectPool(int size)
        {
            for (int idx = 0; idx < size; idx++)
            {
                T newObject = new T();
                newObject.ResetForPool();
                pooledObjects.Enqueue(newObject);
            }
        }

        public T CheckOut()
        {
            return pooledObjects.Dequeue();
        }

        public void CheckIn(T returning)
        {
            returning.ResetForPool();
            pooledObjects.Enqueue(returning);
        }
    }
}
