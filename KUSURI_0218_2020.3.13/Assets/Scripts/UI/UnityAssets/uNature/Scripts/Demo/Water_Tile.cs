using System;
using UnityEngine;

namespace uNature.Demo.UnityStandardAssets
{
    [ExecuteInEditMode]
    public class Water_Tile : MonoBehaviour
    {
        public Planar_Reflection reflection;
        public Water_Base waterBase;


        public void Start()
        {
            AcquireComponents();
        }


        void AcquireComponents()
        {
            if (!reflection)
            {
                if (transform.parent)
                {
                    reflection = transform.parent.GetComponent<Planar_Reflection>();
                }
                else
                {
                    reflection = transform.GetComponent<Planar_Reflection>();
                }
            }

            if (!waterBase)
            {
                if (transform.parent)
                {
                    waterBase = transform.parent.GetComponent<Water_Base>();
                }
                else
                {
                    waterBase = transform.GetComponent<Water_Base>();
                }
            }
        }


#if UNITY_EDITOR
        public void Update()
        {
            AcquireComponents();
        }
#endif


        public void OnWillRenderObject()
        {
            if (reflection)
            {
                reflection.WaterTileBeingRendered(transform, Camera.current);
            }
            if (waterBase)
            {
                waterBase.WaterTileBeingRendered(transform, Camera.current);
            }
        }
    }
}