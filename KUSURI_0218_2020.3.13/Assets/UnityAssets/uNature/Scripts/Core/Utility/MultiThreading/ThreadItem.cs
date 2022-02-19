using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace uNature.Core.Threading
{
    /// <summary>
    /// This class handles assigning parameters before multi-threaded actions that can be called from outside of unity's main thread.
    /// for example :
    /// position.
    /// </summary>
    public class ThreadItem : MonoBehaviour
    {
        /// <summary>
        /// A list that holds all of the thread items in the scene.
        /// </summary>
        public static List<ThreadItem> _threadItems;
        public static List<ThreadItem> threadItems
        {
            get
            {
                if(_threadItems == null)
                {
                    _threadItems = GameObject.FindObjectsOfType<ThreadItem>().ToList();
                }

                return _threadItems;
            }
        }

        /// <summary>
        /// a 3d position which is being updated from the main thread before important multi-threaded actions.
        /// </summary>
        internal Vector3 _threadPosition = new Vector3();
        public Vector3 threadPosition
        {
            get { return _threadPosition; }
            set
            {
                if (_threadPosition != value)
                {
                    _threadPosition = value;

                    OnPositionChanged(value);

                    threadPositionDepth = new Vector2(value.x, value.z);
                }
            }
        }
        /// <summary>
        /// a 2d depth position which is being updated from the main thread before important multi-threaded actions.
        /// 
        /// only includes (X,Z).
        /// </summary>
        [HideInInspector]
        Vector2 _threadPositionDepth = new Vector2(-999, -999);
        public Vector2 threadPositionDepth
        {
            get
            {
                if(_threadPositionDepth == new Vector2(-999, -999))
                {
                    _threadPositionDepth = Vector2.zero;
                }

                return _threadPositionDepth;
            }
            set
            {
                _threadPositionDepth = value;
            }
        }

        //Called when the object is enabled
        protected virtual void OnEnable()
        {
            threadPosition = transform.position;

            if (!threadItems.Contains(this))
            {
                threadItems.Add(this);
            }
        }

        /// <summary>
        /// Called when the object is disabled
        /// </summary>
        protected virtual void OnDisable()
        {
            threadItems.Remove(this);
        }

        /// <summary>
        /// Update...
        /// </summary>
        protected virtual void Update()
        {
            this.threadPosition = transform.position;
        }

        /// <summary>
        /// This method will update this thread item, called externally from unity's main thread.
        /// </summary>
        public virtual void UpdateItem()
        {
            threadPosition = this.transform.position;
        }

        /// <summary>
        /// Called when the item's position changed
        /// </summary>
        protected virtual void OnPositionChanged(Vector3 newPosition)
        {
        }

    }
}