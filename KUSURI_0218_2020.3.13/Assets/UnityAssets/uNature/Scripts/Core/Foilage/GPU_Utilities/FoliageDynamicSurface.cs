using UnityEngine;
using System.Collections.Generic;

using uNature.Wrappers.Linq;

namespace uNature.Core.FoliageClasses
{
    [ExecuteInEditMode]
    public class FoliageDynamicSurface : MonoBehaviour
    {
        #region Variables
        [System.NonSerialized]
        private Vector3 lastReadPosition;
        [System.NonSerialized]
        private Vector3 lastReadScale;

        [System.NonSerialized]
        private bool initiated = false;

        public float updateDistanceDifference = 0.05f;

        [System.NonSerialized]
        Vector3 _worldScale = Vector3.zero;
        Vector3 worldScale
        {
            get
            {
                if(_worldScale == Vector3.zero)
                {
                    Vector3 tempScale = transform.localScale;
                    transform.localScale = Vector3.one;

                    Bounds totalBounds = new Bounds();

                    MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);

                    for(int i = 0; i < renderers.Length; i++)
                    {
                        if(i == 0)
                        {
                            totalBounds = renderers[i].bounds;
                        }
                        else
                        {
                            totalBounds.Encapsulate(renderers[i].bounds);
                        }
                    }

                    _worldScale = totalBounds.size;

                    transform.localScale = tempScale;
                }

                return _worldScale;
            }
        }
        #endregion

        protected virtual void OnEnable()
        {
            if (initiated)
            {
                ApplyPositionChange();
            }


            initiated = true;
        }
        protected virtual void OnDisable()
        {
            try
            {
                List<Collider> colliders = GetComponentsInChildren<Collider>(true).ToList();

                //disable so they wont be included in the change so it can revert changes.
                for (int i = 0; i < colliders.Count; i++)
                {
                    if (!colliders[i].enabled)
                    {
                        colliders.RemoveAt(i);
                        continue;
                    }

                    colliders[i].enabled = false;
                }

                ApplyPositionChange();

                //enable then as they were already enabled.
                for (int i = 0; i < colliders.Count; i++)
                {
                    colliders[i].enabled = true;
                }
            }
            catch
            {
                return;
            }
        }

        protected virtual void Update()
        {
            float positionDistance = Vector3.Distance(transform.position, lastReadPosition);
            float scaleDistance = Vector3.Distance(transform.localScale, lastReadScale);

            if (positionDistance > updateDistanceDifference)
            {
                ApplyPositionChange();
            }
            if(scaleDistance > updateDistanceDifference)
            {
                ApplyScaleChange();
            }
        }

        protected virtual void ApplyPositionChange()
        {
            if (FoliageCore_MainManager.instance == null) return;

            FoliageCore_MainManager mInstance = FoliageCore_MainManager.instance;

            float scaleX = worldScale.x * transform.localScale.x;
            float scaleZ = worldScale.z * transform.localScale.z;

            int x = Mathf.FloorToInt(transform.position.x - scaleX);
            int z = Mathf.FloorToInt(transform.position.z - scaleZ);

            mInstance.UpdateHeights(x, z, Mathf.CeilToInt(scaleX), Mathf.CeilToInt(scaleZ));

            if (initiated)
            {
                //revert old position
                mInstance.UpdateHeights(Mathf.FloorToInt(lastReadPosition.x - scaleX), Mathf.FloorToInt(lastReadPosition.z - scaleZ), Mathf.CeilToInt(scaleX), Mathf.CeilToInt(scaleZ));
            }

            lastReadPosition = transform.position;

            FoliageCore_MainManager.SaveDelayedMaps();
        }
        protected virtual void ApplyScaleChange()
        {
            if (FoliageCore_MainManager.instance == null) return;

            FoliageCore_MainManager mInstance = FoliageCore_MainManager.instance;

            float scaleX = worldScale.x * transform.localScale.x;
            float scaleZ = worldScale.z * transform.localScale.z;

            int x = Mathf.FloorToInt(transform.position.x - scaleX);
            int z = Mathf.FloorToInt(transform.position.z - scaleZ);

            mInstance.UpdateHeights(x, z, Mathf.CeilToInt(scaleX), Mathf.CeilToInt(scaleZ));

            scaleX = worldScale.x * lastReadScale.x;
            scaleZ = worldScale.z * lastReadScale.z;

            x = Mathf.FloorToInt(transform.position.x - scaleX);
            z = Mathf.FloorToInt(transform.position.z - scaleZ);

            //revert old scale
            mInstance.UpdateHeights(x, z, Mathf.CeilToInt(scaleX), Mathf.CeilToInt(scaleZ));

            lastReadScale = transform.localScale;

            FoliageCore_MainManager.SaveDelayedMaps();
        }
    }
}
