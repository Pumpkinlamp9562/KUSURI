using UnityEngine;
using System.Collections;

namespace uNature.Core.Extensions
{
    public class UN_GENA : UNExtension
    {

        public override string AssetName
        {
            get
            {
                return "GENA";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "GeNa is the swiss army knife of spawning \nsystems, enabling rapid creation of gorgeous \nlooking scenes.";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "";
            }
        }

        public override bool IsDefault
        {
            get
            {
                return true;
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/74407";
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
                return "";
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