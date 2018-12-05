using System.Collections.Generic;
using UnityEngine;

namespace SRPGDemo.Utility
{
    public interface IHasRecipe<TRecipe> where TRecipe : ScriptableObject
    {
        void LoadRecipe(TRecipe recipe);
    }

    public class ObjectPool : MonoBehaviour
    {
        public GameObject prefab;

        private Queue<GameObject> usedObjects = new Queue<GameObject>();
        private int poolSize = 0;

        public GameObject GetObject<TRecipe>(TRecipe recipe) where TRecipe : ScriptableObject
        {
            GameObject result = GetObject();

            foreach (IHasRecipe<TRecipe> component in result.GetComponents<IHasRecipe<TRecipe>>())
                component.LoadRecipe(recipe);

            return result;
        }

        public GameObject GetObject()
        {
            if (prefab == null)
            {
                Debug.Log("Pool has no prefab!");
                return null;
            }
            else
            {
                GameObject result;

                if (usedObjects.Count > 0)
                {
                    result = usedObjects.Dequeue();
                    result.SetActive(true);
                }
                else
                {
                    result = Instantiate(prefab);
                    result.AddComponent<PooledObject>();
                    result.GetComponent<PooledObject>().parent = this;
                    result.name += " " + poolSize;
                    poolSize += 1;
                }

                return result;
            }
        }

        public void ReturnObject(GameObject obj)
        {
            if (CheckParent(obj))
            {
                obj.transform.SetParent(transform, false);
                obj.SetActive(false);
                usedObjects.Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        private bool CheckParent(GameObject obj)
        {
            PooledObject pobj = obj.GetComponent<PooledObject>();

            if (pobj == null)
            {
                Debug.Log("Not a pooled object.");
                return false;
            }
            else if (pobj.parent != this)
            {
                Debug.Log("Object from wrong pool.");
                return false;
            }
            else return true;
        }
    }

    public class PooledObject : MonoBehaviour
    {
        public ObjectPool parent = null;
    }
}
