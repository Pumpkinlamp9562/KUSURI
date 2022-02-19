using System;
using System.Collections;
using System.Collections.Generic;

namespace uNature.Core.Utility
{
    public class UNDictionary<T, T1>
    {
        List<T> _Keys = new List<T>();
        public List<T> Keys
        {
            get
            {
                return _Keys;
            }
        }

        List<T1> _Values = new List<T1>();
        public List<T1> Values
        {
            get
            {
                return _Values;
            }
        }

        public void Add(T key, T1 value)
        {
            Keys.Add(key);
            Values.Add(value);
        }

        public void RemoveAt(int index)
        {
            Keys.RemoveAt(index);
            Values.RemoveAt(index);
        }

        public void Remove(T key)
        {
            int targetIndex = TryGetKeyIndex(key);

            if (targetIndex != -1)
            {
                RemoveAt(targetIndex);
            }
        }

        public int TryGetKeyIndex(T key)
        {
            for(int i = 0; i < Keys.Count; i++)
            {
                if (Keys[i].Equals(key))
                    return i;
            }

            return -1;
        }

        public int Count
        {
            get
            {
                return Keys.Count;
            }
        }
    }
}