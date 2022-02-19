using UnityEngine;
using System.Collections;

namespace uNature.Core.FoliageClasses
{
    [ExecuteInEditMode]
    public class TouchBending : Threading.ThreadItem
    {
        public static Vector4[] bendingTargets = new Vector4[20]; // 20 -> max bending targets.

        private static int getFreeID
        {
            get
            {
                for (int i = 0; i < bendingTargets.Length; i++)
                {
                    if (bendingTargets[i].w == 0)
                        return i;
                }

                return -1;
            }
        }

        #region Variables
        [SerializeField]
        private bool _simulateOnEditorTime = true;
        public bool simulateOnEditorTime
        {
            get
            {
                return _simulateOnEditorTime;
            }
            set
            {
                if (_simulateOnEditorTime != value)
                {
                    _simulateOnEditorTime = value;

                    if(!_simulateOnEditorTime)
                    {
                        OnDisable();
                    }
                    else
                    {
                        OnEnable();
                    }
                }
            }
        }

        [SerializeField]
        private float _radius = 1;
        public float radius
        {
            get
            {
                return _radius;
            }
            set
            {
                if (_radius != value)
                {
                    _radius = value;

                    UpdateStaticBendingCache_Radius();
                    UpdateStaticBendingCache_Data();
                }
            }
        }

        [SerializeField]
        private float _seekingRange = 50;
        public float seekingRange
        {
            get
            {
                return _seekingRange;
            }
            set
            {
                if(_seekingRange != value)
                {
                    _seekingRange = value;

                    CalculateTouchBending(transform.position, true);
                }
            }
        }
        #endregion

        #region Parameters
        [System.NonSerialized]
        public bool inBounds = false;

        private int _id = 0;
        public int id
        {
            get
            {
                return _id;
            }
            private set
            {
                _id = value;
            }
        }

        public bool simulate
        {
            get
            {
                return Application.isPlaying || simulateOnEditorTime;
            }
        }

        [System.NonSerialized]
        Vector3 lastChangedPosition;
        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!simulate) return;

            id = getFreeID;

            if (id == -1)
            {
                Debug.LogError("No more space for touch bending targets, please increase the max bending objects amount! -> " + transform.name);
                this.enabled = false;

                return;
            }

            UpdateStaticBendingCache_Radius();
            UpdateStaticBendingCache_Data();

            FoliageCore_MainManager.OnFoliageManagerAssignedEvent += FoliageCore_MainManager_OnFoliageManagerAssignedEvent;

            OnPositionChanged(transform.position);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (id == -1) return;

            bendingTargets[id].w = 0;

            UpdateStaticBendingCache_Data();

            id = -1;

            FoliageCore_MainManager.OnFoliageManagerAssignedEvent -= FoliageCore_MainManager_OnFoliageManagerAssignedEvent;
        }

        private void FoliageCore_MainManager_OnFoliageManagerAssignedEvent(FoliageCore_MainManager instance)
        {
            if (instance == null) return;

            UpdateStaticBendingCache_Radius();
            UpdateStaticBendingCache_Data();

            CalculateTouchBending(transform.position, true);
        }

        private void UpdateStaticBendingCache_Radius()
        {
            if (id == -1) return;

            bendingTargets[id].w = radius;
        }

        private void UpdateStaticBendingCache_Data()
        {
            #if UNITY_5_4_OR_NEWER
            if(FoliageCore_MainManager.instance == null) return;

            FoliageCore_MainManager.instance.propertyBlock.SetVectorArray("_InteractionTouchBendedInstances", bendingTargets);
            #endif
        }

        private bool CalculateBounds()
        {
            FoliageReceiver fReceiver;

            for(int i = 0; i < FoliageReceiver.FReceivers.Count; i++)
            {
                fReceiver = FoliageReceiver.FReceivers[i];

                if (!fReceiver.isGrassReceiver) continue;

                if (Vector3.Distance(fReceiver.threadPosition, threadPosition) <= seekingRange) return true;
            }

            return false;
        }

        protected virtual void OnDrawGizmos()
        {
            // touch bending radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);

            //Draw AOI
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, seekingRange);
        }

        protected override void OnPositionChanged(Vector3 newPosition)
        {
            base.OnPositionChanged(newPosition);

            CalculateTouchBending(newPosition, false);
        }

        private void CalculateTouchBending(Vector3 newPosition, bool force)
        {
            if (!simulate) return;

            float distance = Vector3.Distance(lastChangedPosition, newPosition);

            if (distance > 0.02f || force)
            {
                inBounds = CalculateBounds();

                if (!inBounds)
                {
                    if(id != -1)
                    {
                        bendingTargets[id].w = 0;
                        UpdateStaticBendingCache_Data();

                        id = -1;
                    }

                    return;
                }
                else
                {
                    if(id == -1)
                    {
                        id = getFreeID;

                        if (id == -1) return; // if no ids left return

                        UpdateStaticBendingCache_Radius();
                    }
                }

                bendingTargets[id].x = newPosition.x;
                bendingTargets[id].y = newPosition.y;
                bendingTargets[id].z = newPosition.z;

                UpdateStaticBendingCache_Data();

                lastChangedPosition = newPosition;
            }
        }
    }
}
