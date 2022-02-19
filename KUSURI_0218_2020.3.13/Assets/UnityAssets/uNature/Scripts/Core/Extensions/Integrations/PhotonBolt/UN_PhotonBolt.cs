using UnityEngine;
using System.Collections;

namespace uNature.Core.Extensions
{
    public class UN_PhotonBolt : UNExtension
    {
        public override string AssetName
        {
            get
            {
                return "Photon Bolt";
            }
        }
        public override string AssetNameSpace
        {
            get
            {
                return "UN_PhotonBolt";
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
                return "UN_PhotonBolt.pdf";
            }
        }
        public override string AssetDescription
        {
            get
            {
                return "Build multiplayer games in Unity without having to \nknow the details of networking or write\nany complex networking code.";
            }
        }
        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/41330";
            }
        }
        public override string AssetLogoName
        {
            get
            {
                return "PhotonBolt_Logo";
            }
        }

    }
}