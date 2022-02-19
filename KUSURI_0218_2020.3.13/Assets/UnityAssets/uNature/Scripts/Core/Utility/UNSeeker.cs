using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uNature.Core.Targets;
using uNature.Wrappers.Linq;
using uNature.Core.FoliageClasses;

namespace uNature.Core.Seekers
{
    /// <summary>
    /// Seekers are basically GameObjects in the scene which should interact with the objects in the game.
    /// </summary>
    [System.Serializable]
    public class UNSeeker : FoliageReceiver
    {
        #region Variables
        /// <summary>
        /// What was the last position our AOI was updated on?
        /// </summary>
        protected Vector3 lastMovement = Vector3.zero;

        /// <summary>
        /// Was the distance check done on init?
        /// </summary>
        private bool initialTreesDetectionDone = false;

        /// <summary>
        /// How far will it look ?
        /// </summary>
        public float seekingDistance = 50f;

        /// <summary>
        /// After how much distance will it update the trees.
        /// </summary>
        [SerializeField]
        protected float _treesCheckDistance = 5f;
        public float treesCheckDistance
        {
            get
            {
                return _treesCheckDistance;
            }
            set
            {
                _treesCheckDistance = value;
            }
        }

        /// <summary>
        /// Disable this if you want to do your own trees logic.
        /// </summary>
        public bool attackTrees = true;

        /// <summary>
        /// Ignore layer for tree attack.
        /// </summary>
        public int raycastMask = 1;

        /// <summary>
        /// Raycast range for tree attack.
        /// </summary>
        public float raycastDistance = 10;
        #endregion

        /// <summary>
        /// Check for movement.
        /// </summary>
        protected override void Update()
        {
            base.Update();

            CheckTargetsOnMove();
            HarvestChecks();
        }

        /// <summary>
        /// Checks the targets when the character moved enoughed.
        /// </summary>
        protected virtual void CheckTargetsOnMove()
        {
            if (Vector3.Distance(transform.position, lastMovement) > treesCheckDistance || !initialTreesDetectionDone)
            {
                lastMovement = transform.position;
                initialTreesDetectionDone = true;

                UNTarget.CheckTargets(this, seekingDistance);
            }
        }

        /// <summary>
        /// Try to harvest trees.
        /// </summary>
        protected virtual void HarvestChecks()
        {
            if (attackTrees && playerCamera != null && Input.GetMouseButtonDown(0))
            {
                Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                RaycastHit[] hits;
                RaycastHit hit;

                hits = Physics.RaycastAll(ray, raycastDistance, raycastMask).OrderBy(x => x.distance).ToArray();

                if (hits.Length > 0)
                {
                    hit = hits[0];

                    if (hit.transform.GetComponentInParent<uNature.Core.Pooling.IHarvestableItem>() != null)
                    {
                        hit.transform.GetComponentInParent<uNature.Core.Pooling.IHarvestableItem>().Hit(20);
                    }
                }
            }
        }

        /// <summary>
        /// Called on start, initiate initial check targets.
        /// </summary>
        public virtual IEnumerator Start()
        {
            lastMovement = transform.position;

            yield return new WaitForSeconds(0.1f);

            if (!Application.isPlaying) yield break;

            lastMovement = transform.position;
            UNTarget.CheckTargets(this, seekingDistance);
        }
    }
}