using UnityEngine;
using System.Collections;

namespace uNature.Core.Extensions
{
    public class UN_MapMagic : UNExtension
    {
        public override string AssetName
        {
            get
            {
                return "Map Magic";
            }
        }
        public override string AssetNameSpace
        {
            get
            {
                return "UN_MapMagic";
            }
        }
        public override string PublisherName
        {
            get
            {
                return "Denis Pahunov";
            }
        }
        public override bool Featured
        {
            get
            {
                return false;
            }
        }
        public override string AssetDocumentationName
        {
            get
            {
                return "";
            }
        }
        public override string AssetDescription
        {
            get
            {
                return "A node based procedural and infinite game \nmap generator.";
            }
        }
        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/56762";
            }
        }
        public override string AssetLogoName
        {
            get
            {
                return "UN_MapMagic_Icon";
            }
        }
    }
}
