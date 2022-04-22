using UnityEngine;
using System.Collections;

namespace uNature.Core.Extensions
{
    public class UN_ForgeNetworking : UNExtension
    {

        public override string AssetName
        {
            get
            {
                return "Forge Networking";
            }
        }

        public override string AssetDescription
        {
            get
            {
                return "Ever wanted to make a multiplayer game? \nYou've never seen a networking library quite \nlike this before! Find out why so many people \nare leaving the other network solutions to \nfinally be free to build the multiplayer games \nthey want without limitations! Come join the \ncommunity for the fastest growing \nnetworking solution on the Asset Store!";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "UN_ForgeNetworking";
            }
        }

        public override string AssetStoreAdress
        {
            get
            {
                return "https://www.assetstore.unity3d.com/en/#!/content/38344";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Bearded Man Studios";
            }
        }

        public override string AssetLogoName
        {
            get
            {
                return "ForgeNetworking_Logo";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "UN_ForgeNetworking.pdf";
            }
        }

        [MethodHelper()]
        public void CreateManager()
        {
            #if UN_ForgeNetworking
            var instance = GameObject.FindObjectOfType<uNature.Extensions.ForgeNetworking.ForgeNetworkingCallbackManager>();

            if(instance == null)
            {
                GameObject go = new GameObject("UN Networking Manager");
                go.AddComponent<uNature.Extensions.ForgeNetworking.ForgeNetworkingCallbackManager>();
            }

            #endif
        }

    }
}