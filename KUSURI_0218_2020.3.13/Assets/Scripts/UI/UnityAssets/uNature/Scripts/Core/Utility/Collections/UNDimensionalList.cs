using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace uNature.Core.Collections
{
    /// <summary>
    /// A 2 dimensional list which is used by certain mechanics in uNature.
    /// </summary>
    public class UNDimensionalList<T>
    {
        /// <summary>
        /// a two dimensional list.
        /// </summary>
        List<List<T>> twoDimensionalList = new List<List<T>>();

        /// <summary>
        /// Checks if the list contains a certain key.
        /// </summary>
        public bool ContainsKey(int key)
        {
            return key < twoDimensionalList.Count;
        }

        /// <summary>
        /// Does the two dimensional list contain this value?
        /// </summary>
        /// <param name="value">the value</param>
        /// <returns>is it contained ?</returns>
        public bool ContainsValue(T value)
        {
            for(int i = 0; i < twoDimensionalList.Count; i++)
            {
                if(twoDimensionalList[i].Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the stashed list.
        /// </summary>
        /// <param name="index">index</param>
        /// <returns></returns>
        public List<T> this[int index]
        {
            get
            {
                return twoDimensionalList[index];
            }
            set
            {
                twoDimensionalList[index] = value;
            }
        }

        /// <summary>
        /// Try to add a key.
        /// </summary>
        /// <param name="value">the value</param>
        public void TryAddKey(List<T> value)
        {
            twoDimensionalList.Add(value);
        }

        /// <summary>
        /// Count of the two dimensional list elements.
        /// </summary>
        public int Count
        {
            get { return twoDimensionalList.Count; }
        }

        /*
        /// <summary>
        /// Convert this to an Int array.
        /// </summary>
        /// <returns></returns>
        public T[,] ToArray(T defaultIfMissing)
        {
            if(twoDimensionalList.Count == 0) return null;

            int count = twoDimensionalList[0].Count;

            T[,] array = new T[Count, count];
            List<T> current;

            for(int i = 0; i < Count; i++)
            {
                for(int b = 0; b < count; b++)
                {
                    current = twoDimensionalList[i];

                    if (current.Count < count)
                    {
                        int difference = count - current.Count;

                        for(int c = 0; c < difference; c++)
                        {
                            current.Add(defaultIfMissing);
                        }
                    }

                    array[i, b] = current[b];
                }
            }

            return array;
        }
        */
    }
}
