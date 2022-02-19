using UnityEngine;
using System.Collections;

using uNature.Core.Terrains;
using uNature.Core.Pooling;

#if UN_UFPS
using uNature.Extensions.UFPS;
#endif

namespace uNature.Core.Extensions
{
    public class UN_UFPS : UNExtension
    {
        public override string AssetName
        {
            get { return "UFPS"; }
        }

        public override string AssetDescription
        {
            get
            {
                return "UFPS is a professional FPS base platform for \nUnity. One of the longest-running and most \npopular titles of the Unity Asset Store, it’s \nknown for smooth controls and fluid, \nrealtime-generated camera and weapon \nmotion. Since 2012 it has been steadily \nexpanded, supported and refactored with a \nfocus on robust, generic FPS features.";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "UFPS_Logo";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "UN_UFPS";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/2943";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Opsive";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "UN_UFPS.pdf";
            }
        }

        [MethodHelper]
        public void ApplyOnCurrentPool()
        {
            #if UN_UFPS
            foreach(var terrain in GameObject.FindObjectsOfType<UNTerrain>())
            {
                foreach(var PoolItem in terrain.Pool.items)
                {
                    if (PoolItem.gameObject.GetComponent<UN_DamageHandler>() == null)
                        PoolItem.gameObject.AddComponent<UN_DamageHandler>();
                }
            }
            #endif
        }

    }
}