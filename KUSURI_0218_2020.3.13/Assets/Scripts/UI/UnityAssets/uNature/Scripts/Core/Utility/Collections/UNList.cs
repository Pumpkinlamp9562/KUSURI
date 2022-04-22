using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace uNature.Core.Collections
{
    /// <summary>
    /// A custom list which is used on some important interfaces in UN.
    /// </summary>
    /// <typeparam name="T">the list type.</typeparam>
    [System.Serializable]
    public class UNList<T>
    {
        [SerializeField]
        private List<T> list = new List<T>();

        /// <summary>
        /// Add an item to the list.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }

        /// <summary>
        /// Remove an item from the list.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            list.Remove(item);
        }

        /// <summary>
        /// Get list count
        /// </summary>
        public int Count
        {
            get { return list.Count; }
        }

        /// <summary>
        /// Get an element from the list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }

        /// <summary>
        /// Get a similar instance by a custom Equals which needs to be initialized on the item.
        /// </summary>
        /// <param name="similarItem"></param>
        /// <returns></returns>
        public T TryGet(System.Object similarItem)
        {
            T item;

            for(int i = 0; i < list.Count; i++)
            {
                item = list[i];

                if (item.Equals(similarItem))
                {
                    return item;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Is this item contained in the list?
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(System.Object item)
        {
            for(int i = 0; i < list.Count; i++)
            {
                if(list[i].Equals(item))
                {
                    return true;
                }
            }

            return false;
        }
    }
}