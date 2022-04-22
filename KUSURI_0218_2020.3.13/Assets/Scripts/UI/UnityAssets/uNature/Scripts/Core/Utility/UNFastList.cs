using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uNature.Core.Utility
{
    /// <summary>
    /// A list that requires you to set 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UNFastList<T>
    {
        internal T[] arrayAllocation;
        private int count;

        public int Count
        {
            get
            {
                return count;
            }
        }
        
        public UNFastList(int maxCapacity)
        {
            arrayAllocation = new T[maxCapacity];
            count = 0;
        }

        public void Add(T item)
        {
            arrayAllocation[count] = item;
            count++;
        }

        public void Clear()
        {
            count = 0;
            System.Array.Clear(arrayAllocation, 0, arrayAllocation.Length);
        }
    }
}
