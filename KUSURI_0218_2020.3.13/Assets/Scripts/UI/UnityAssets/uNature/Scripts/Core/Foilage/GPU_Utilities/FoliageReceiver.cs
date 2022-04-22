using UnityEngine;
using System.Collections.Generic;

using uNature.Core.Utility;
using uNature.Core.FoliageClasses.Interactions;

namespace uNature.Core.FoliageClasses
{
    [ExecuteInEditMode]
    public class FoliageReceiver : Threading.ThreadItem
    {
        public readonly static List<FoliageReceiver> FReceivers = new List<FoliageReceiver>();

        #region Variables
        [System.NonSerialized]
        FoliageCore_Chunk[] _neighbors = null;
        public FoliageCore_Chunk[] neighbors
        {
            get
            {
                if (_neighbors == null)
                {
                    _neighbors = new FoliageCore_Chunk[9];

                    _neighbors = UNStandaloneUtility.GetFoliageChunksNeighbors(transform.position - FoliageCore_MainManager.instance.transform.position, _neighbors);
                }

                return _neighbors;
            }
        }

        public FoliageCore_Chunk middleFoliageChunkFromNeighbors
        {
            get
            {
                return neighbors[4];
            }
        }

        [SerializeField]
        protected float _grassCheckDistance = 20f;
        public float grassCheckDistance
        {
            get
            {
                return _grassCheckDistance;
            }
            set
            {
                _grassCheckDistance = value;
            }
        }

        private Vector3 lastCheckedPosition;
        private bool wasPositionChecked = false;

        [SerializeField]
        Camera _playerCamera;
        public Camera playerCamera
        {
            get
            {
                if (_playerCamera == null)
                {
                    _playerCamera = GetComponentInChildren<Camera>();
                }

                return _playerCamera;
            }
            set
            {
                _playerCamera = value;
            }
        }

        public bool isGrassReceiver = true;
        #endregion

        #region RenderingQueue
        [System.NonSerialized]
        RenderingQueue_InteractionReceiver _queueInstance = null;
        public RenderingQueue_InteractionReceiver queueInstance
        {
            get
            {
                if (_queueInstance == null) // not assigned
                {
                    _queueInstance = new RenderingQueue_InteractionReceiver();
                    _queueInstance.transform = transform;
                }

                return _queueInstance;
            }
            internal set
            {
                _queueInstance = value;
            }
        }
        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            FReceivers.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            FReceivers.Remove(this);
        }

        protected override void Update()
        {
            if (!Application.isPlaying) return;

            base.Update();

            if (isGrassReceiver && FoliageCore_MainManager.instance != null && FoliageCore_MainManager.instance.enabled)
            {
                if ((!wasPositionChecked || Vector3.Distance(lastCheckedPosition, transform.position) >= grassCheckDistance))
                {
                    _neighbors = UNStandaloneUtility.GetFoliageChunksNeighbors(transform.position - FoliageCore_MainManager.instance.transform.position, _neighbors);

                    wasPositionChecked = true;
                    lastCheckedPosition = transform.position;
                }

                queueInstance.camera = playerCamera;
                queueInstance.CheckPositionChange();
                RenderingPipielineUtility.RenderQueue(queueInstance, playerCamera);
            }
        }
    }
}