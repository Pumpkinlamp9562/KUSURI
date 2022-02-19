using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uNature.Core.ClassExtensions;
using uNature.Core.Threading;
using System.Linq;

namespace uNature.Core.Pooling
{
    /// <summary>
    /// An abstract class that handles the Pool items.
    /// </summary>
    public abstract class PoolItem : ThreadItem
    {
        static System.Type[] _PoolTypes;
        /// <summary>
        /// All the Pool types in the assembly.
        /// </summary>
        public static System.Type[] PoolTypes
        {
            get
            {
                if(_PoolTypes == null)
                {
                    _PoolTypes = System.Reflection.Assembly.GetAssembly(typeof(PoolItem)).GetTypes().Where(x => x.InheritsFrom(typeof(PoolItem))).ToArray();
                }

                return _PoolTypes;
            }
        }

        /// <summary>
        /// What Pool are we belonged to?
        /// </summary>
        [HideInInspector]
        public Pool Pool;

        /// <summary>
        /// An gameobject reference which can be used on a different thread.
        /// </summary>
        [HideInInspector]
        public GameObject _gameObject;

        /// <summary>
        /// is the item currently used?
        /// </summary>
        [HideInInspector]
        public bool used;

        /// <summary>
        /// Is this Pool item locked? If so, dont let it return back to Pool unless forced.
        /// </summary>
        [HideInInspector]
        public bool locked;

        /// <summary>
        /// The Pool item unique id, which is used to identify the item. (not including offset)
        /// </summary>
        [HideInInspector]
        public int realItemID;

        /// <summary>
        /// The offset of the item id which allows the item id to be more unique. Can be left 0.
        /// </summary>
        [HideInInspector]
        public int itemID_Offset;

        /// <summary>
        /// The Pool item unique id, which is used to identify the item. (including offset)
        /// </summary>
        public int itemID
        {
            get { return realItemID + itemID_Offset; }
        }

        /// <summary>
        /// What is the uid of the item we are attached to.
        /// </summary>
        [HideInInspector]
        public int uid = -1;

        /// <summary>
        /// Called on awake.
        /// </summary>
        public virtual void Awake()
        {
            if (!used && !locked)
                gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when the object is enabled.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        /// <summary>
        /// Called when the object is disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
        }

        /// <summary>
        /// Called when the item has been Pooled.
        /// </summary>
        public virtual void OnPool()
        {
            UNThreadManager.instance.RunOnUnityThread(new ThreadTask(() => {
                try
                {
                    gameObject.SetActive(true);
                }
                catch { }
            }));
        }
        /// <summary>
        /// Called when the item has returned to the Pool
        /// </summary>
        public virtual void OnReturnedToPool()
        {
            used = false;
            locked = false;
            uid = -1;

            ThreadTask<PoolItem> task = new ThreadTask<PoolItem>((PoolItem item) =>
            {
                if (!item.used)
                {
                    item.gameObject.SetActive(false);
                }
            }, this);

            UNThreadManager.instance.RunOnUnityThread(task);
        }
        /// <summary>
        /// Called when the item has been created.
        /// </summary>
        public virtual void OnCreated()
        {
        }

        /// <summary>
        /// Move the item to a certain position. NOTE: in order to move the item, use this method and DONT change the position externally!!
        /// </summary>
        /// <param name="position">target position.</param>
        public virtual void MoveItem(Vector3 position)
        {
            gameObject.SetActive(true);
        }
    }
}
