using UnityEngine;
using System.Collections;

namespace uNature.Core.Extensions
{
    public class UN_GAIA : UNExtension
    {

        public override string AssetName
        {
            get
            {
                return "GAIA";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "GAIA is a system that enables rapid and \nprecise creation of gorgeous looking terrains \nand scenes.";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "UN_GAIA";
            }
        }

        public override bool IsDefault
        {
            get
            {
                return false;
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/42618";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Adam Goodrich";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "GAIA_Logo";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "";
            }
        }
    }
}