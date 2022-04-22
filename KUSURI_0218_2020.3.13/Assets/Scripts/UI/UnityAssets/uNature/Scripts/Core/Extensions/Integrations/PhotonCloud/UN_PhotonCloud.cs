using UnityEngine;
using System.Collections;

namespace uNature.Core.Extensions
{
    public class UN_PhotonCloud : UNExtension
    {
        public override string AssetName
        {
            get
            {
                return "Photon Cloud";
            }
        }
        public override string AssetNameSpace
        {
            get
            {
                return "UN_PhotonCloud";
            }
        }
        public override string PublisherName
        {
            get
            {
                return "Exit Games";
            }
        }
        public override string AssetDocumentationName
        {
            get
            {
                return "‏‏‏‏UN_PhotonCloud.pdf";
            }
        }
        public override string AssetDescription
        {
            get
            {
                return "The ease-of-use of Unity's Networking plus \nthe performance and reliability of the \nPhoton Cloud.";
            }
        }
        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/1786";
            }
        }
        public override string AssetLogoName
        {
            get
            {
                return "PhotonCloud_Logo";
            }
        }

        [MethodHelper()]
        public void CreateManager()
        {
            #if UN_PhotonCloud
            var instance = GameObject.FindObjectOfType<uNature.Extensions.PhotonCloud.PhotonCloudCallbackManager>();

            if(instance == null)
            {
                GameObject go = new GameObject("UN Networking Manager");
                go.AddComponent<uNature.Extensions.PhotonCloud.PhotonCloudCallbackManager>();
            }

            #endif
        }

    }
}