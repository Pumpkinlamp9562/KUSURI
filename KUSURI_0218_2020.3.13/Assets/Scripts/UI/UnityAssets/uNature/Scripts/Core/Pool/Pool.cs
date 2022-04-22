using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using uNature.Core.Seekers;

namespace uNature.Core.Pooling
{
    /// <summary>
    /// A class that manages the Pooling of the system,
    /// Which allows huge runtime performance increase.
    /// </summary>
    public class Pool : MonoBehaviour
    {
        /// <summary>
        /// A list that holds all of the Pool items in our Pool.
        /// </summary>
        public List<PoolItem> items = new List<PoolItem>();

        /// <summary>
        /// Who created this Pool?
        /// </summary>
        public GameObject owner;

        /// <summary>
        /// Add an item to the Pool.
        /// </summary>
        /// <param name="item">the item.</param>
        /// <param name="itemID">The targeted item id</param>
        /// <param name="itemID_Offset">The offset of the item id to make it unique.</param>
        public void AddToPool(PoolItem item, int itemID, int itemID_Offset)
        {
            if (item.GetType().IsAbstract)
            {
                Debug.LogError("Cant add to Pool type : " + item.GetType() + " As it's an abstract class.");
                return;
            }

            items.Add(item);

            item.used = false;
            item.realItemID = itemID;
            item.itemID_Offset = itemID_Offset;
            item.Pool = this;

            item._gameObject = item.gameObject;

            item.gameObject.SetActive(false);

            item.OnCreated();
        }
        /// <summary>
        /// Remove an item from the Pool
        /// </summary>
        /// <param name="item">the item.</param>
        public void RemoveFromPool(PoolItem item)
        {
            items.Remove(item);
        }

        /// <summary>
        /// Return a certain item to Pool.
        /// </summary>
        /// <param name="item">the item.</param>\
        /// <param name="force">making force true, will make the system ignore the locked state of the item. (if exists)</param>
        public void ReturnToPool(PoolItem item, bool force)
        {
            if (item.locked && !force) return;

            item.OnReturnedToPool();
        }

        /// <summary>
        /// Reset a certain item which is on a certain UID
        /// <param name="uid">the targeted UID</param>
        /// <param name="forceReset">Force reset will make it ignore the locked state of the item.</param>
        /// </summary>
        public void TryResetOnUID(int uid, bool forceReset)
        {
            PoolItem item;

            for(int i = 0; i < items.Count; i++)
            {
                item = items[i];

                if (item.uid == uid)
                {
                    ReturnToPool(items[i], forceReset);
                    return;
                }
            }
        }

        /// <summary>
        /// Try to Pool an item, will return null if no target is found.
        /// </summary>
        /// <param name="itemUID">the uid of the item (without offset)</param>
        /// <param name="itemID_Offset">the offset of the required item id</param>
        /// <param name="uid">a unique id of the object which will be attached to this game object. ( HAS TO BE UNIQUE...)</param>
        /// <param name="locked">if the Pool item is locked, it wont be able to return to Pool unless its unlocked.</param>
        /// <returns>A Pool item.</returns>
        public T TryPool<T>(int itemUID, int itemID_Offset, int uid, bool locked, bool activatePool) where T : PoolItem
        {
            PoolItem current = null;

            for(int i = 0; i < items.Count; i++)
            {
                current = items[i];

                if (!current.used && current.itemID == (itemUID + itemID_Offset) && !IsAlreadyPooled(uid))
                {
                    PoolItem(current, locked, uid, activatePool);

                    return current as T;
                }
            }

            return null;
        }

        /// <summary>
        /// Get Pool of a certain item
        /// </summary>
        /// <param name="itemUID"></param>
        /// <param name="itemID_Offset"></param>
        /// <returns></returns>
        public List<PoolItem> GetPoolOfItem(int itemUID, int itemID_Offset)
        {
            List<PoolItem> tempItems = new List<PoolItem>();

            PoolItem current = null;

            for (int i = 0; i < items.Count; i++)
            {
                current = items[i];

                if (current.itemID == (itemUID + itemID_Offset))
                {
                    tempItems.Add(current);
                }
            }

            return tempItems;
        }

        /// <summary>
        /// Pool the certain item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PoolItem"></param>
        public void PoolItem(PoolItem PoolItem, bool locked, int uid, bool activatePool)
        {
            PoolItem.locked = locked;
            PoolItem.used = true;
            PoolItem.uid = uid;

            if (activatePool)
            {
                PoolItem.OnPool();
            }
        }

        /// <summary>
        /// This method will find and reset far away items to be "recycled"
        /// </summary>
        public void ResetFarAway()
        {
            List<PoolItem> items = this.items;

            UNSeeker seeker;
            PoolItem item;
            int farAwayCount;

            for (int i = 0; i < items.Count; i++)
            {
                farAwayCount = 0;

                item = items[i];

                for (int b = 0; b < UNSeeker.FReceivers.Count; b++)
                {
                    seeker = UNSeeker.FReceivers[b] as UNSeeker;

                    if (!item.used || item.locked) continue;

                    if (Vector2.Distance(item.threadPositionDepth, seeker.threadPositionDepth) > (seeker.seekingDistance))
                    {
                        farAwayCount++;
                    }

                    if (farAwayCount == UNSeeker.FReceivers.Count)
                    {
                        ReturnToPool(item, false);
                    }
                }
            }

        }

        /// <summary>
        /// Check if a certain uid is already Pooled.
        /// </summary>
        /// <param name="uid">the uid of the targeted item</param>
        /// <returns>is this item already Pooled?</returns>
        public bool IsAlreadyPooled(int uid)
        {
            for(int i = 0; i < items.Count; i++)
            {
                if (items[i].uid == uid)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Try to get an object from the Pool with a certain component.
        /// </summary>
        /// <typeparam name="T">the type of the component</typeparam>
        /// <returns></returns>
        public T TryGetType<T>() where T : Component
        {
            T instance;

            for(int i = 0; i < items.Count; i++)
            {
                instance = items[i].GetComponent<T>();
                
                if(instance != null)
                {
                    return instance;
                }
            }

            return null;
        }

        /// <summary>
        /// Create a new Pool
        /// </summary>
        /// <param name="name">the Pool name (Without Pool at the end)</param>
        /// <param name="requester">who is the owner of this Pool</param>
        /// <returns>the newely created Pool.</returns>
        public static Pool CreatePool(string name, GameObject requester)
        {
            GameObject go = new GameObject(name);
            var PoolComponent = go.AddComponent<Pool>();
            PoolComponent.owner = requester;
            PoolComponent.transform.parent = requester.transform.parent;

            return PoolComponent;
        }

        /// <summary>
        /// Remove Pool duplications.
        /// </summary>
        /// <param name="name"></param>
        public static void RemoveDuplications(string name)
        {
            Pool[] Pools = GameObject.FindObjectsOfType<Pool>();

            for(int i = 0; i < Pools.Length; i++)
            {
                if (Pools[i].name == name)
                {
                    GameObject.DestroyImmediate(Pools[i].gameObject);
                }
            }
        }
    }
}