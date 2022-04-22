using UnityEngine;
using System.Collections;

namespace uNature.Core.Extensions
{
    public class UN_TerrainComposer : UNExtension
    {
        public override string AssetName
        {
            get
            {
                return "Terrain Composer 2";
            }
        }
        public override string AssetNameSpace
        {
            get
            {
                return "UN_TerrainComposer2";
            }
        }
        public override string PublisherName
        {
            get
            {
                return "Nathaniel Doldersum";
            }
        }
        public override bool Featured
        {
            get
            {
                return true;
            }
        }
        public override bool IsDefault
        {
            get
            {
                return true;
            }
        }
        public override string AssetDescription
        {
            get
            {
                return "TerrainComposer2 is a complete new terrain \ntool developed from scratch in C# and CG \ncompute shaders. All feedback was taken \nfrom the last 3.5 years from TC1 to create a \nbrand new super powerful terrain tool using \nthe latest technology available in Unity. ";
            }
        }
        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/65563";
            }
        }
        public override string AssetLogoName
        {
            get
            {
                return "UN_TerrainComposer2";
            }
        }
    }
}
