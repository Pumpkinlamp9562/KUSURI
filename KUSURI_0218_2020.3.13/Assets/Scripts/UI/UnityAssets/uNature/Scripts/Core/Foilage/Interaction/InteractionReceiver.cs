using UnityEngine;
using System.Collections;

using uNature.Core.Utility;

namespace uNature.Core.FoliageClasses.Interactions
{
    public class RenderingQueue_InteractionReceiver : RenderingQueueReceiver
    {
        #region Variables
        [SerializeField]
        private FoliageResolutions _interactionMapResolution = FoliageResolutions._256;
        public FoliageResolutions interactionMapResolution
        {
            get
            {
                return _interactionMapResolution;
            }
            set
            {
                if(_interactionMapResolution != value)
                {
                    _interactionMapResolution = value;
                    _interactionMapMultiplier = -1;
                }
            }
        }

        [SerializeField]
        private FoliageResolutions _interactionMapSize = FoliageResolutions._32;
        public FoliageResolutions interactionMapSize
        {
            get
            {
                return _interactionMapSize;
            }
            set
            {
                if(_interactionMapSize != value)
                {
                    _interactionMapSize = value;
                    _interactionMapMultiplier = -1;
                }
            }
        }

        [System.NonSerialized]
        private float _interactionMapMultiplier = -1;
        internal float interactionMapMultiplier
        {
            get
            {
                if(_interactionMapMultiplier == -1)
                {
                    _interactionMapMultiplier = (float)interactionMapSize / (int)interactionMapResolution;
                }

                return _interactionMapMultiplier;
            }
        }

        private int _interactionMapExtents = -1;
        public int interactionMapExtents
        {
            get
            {
                if(_interactionMapExtents == -1)
                {
                    _interactionMapExtents = (int)interactionMapSize / 2;
                }

                return _interactionMapExtents;
            }
        }

        public Vector3 interactionCenter
        {
            get
            {
                Vector3 center = transform.position;

                center.x += interactionMapExtents;
                center.z += interactionMapExtents;

                return center;
            }
        }

        [System.NonSerialized]
        private InteractionMap _interactionMap;
        public InteractionMap interactionMap
        {
            get
            {
                if (_interactionMap == null)
                {
                    _interactionMap = InteractionMap.CreateMap(this);
                }

                return _interactionMap;
            }
        }
        #endregion

        /// <summary>
        /// Called when the grass is updated, let's update the interaction map.
        /// </summary>
        protected override void OnUpdated()
        {
            //BaseInteraction.RecalculateMap(this);
        }
    }
}
