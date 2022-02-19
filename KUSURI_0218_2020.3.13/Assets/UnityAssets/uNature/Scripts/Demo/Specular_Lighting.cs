using System;
using UnityEngine;

namespace uNature.Demo.UnityStandardAssets
{
    [RequireComponent(typeof(Water_Base))]
    [ExecuteInEditMode]
    public class Specular_Lighting : MonoBehaviour
    {
        public Transform specularLight;
        private Water_Base m_WaterBase;


        public void Start()
        {
            m_WaterBase = (Water_Base)gameObject.GetComponent(typeof(Water_Base));
        }


        public void Update()
        {
            if (!m_WaterBase)
            {
                m_WaterBase = (Water_Base)gameObject.GetComponent(typeof(Water_Base));
            }

            if (specularLight && m_WaterBase.sharedMaterial)
            {
                m_WaterBase.sharedMaterial.SetVector("_WorldLightDir", specularLight.transform.forward);
            }
        }
    }
}