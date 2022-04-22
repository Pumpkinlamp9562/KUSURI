using UnityEngine;
using System.Collections;

namespace uNature.Core.Extensions
{
    public class UN_WorldStreamer : UNExtension
    {
        public override string AssetName
        {
            get
            {
                return "World Streamer";
            }
        }
        public override string AssetNameSpace
        {
            get
            {
                return "UN_WorldStreamer";
            }
        }
        public override string PublisherName
        {
            get
            {
                return "NatureManufacture";
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
                return "World Streamer is a memory streaming \nsystem. By using it you are able to stream \nwhole your game from a disc in any axis and space.";
            }
        }
        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/36486";
            }
        }
        public override string AssetLogoName
        {
            get
            {
                return "WorldStreamer_UN_Logo";
            }
        }
    }
}
