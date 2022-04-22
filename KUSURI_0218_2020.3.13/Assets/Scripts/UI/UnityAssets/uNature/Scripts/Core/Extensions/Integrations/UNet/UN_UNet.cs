using UnityEngine;
using System.Collections;

namespace uNature.Core.Extensions
{
    public class UN_UNet : UNExtension
    {

        public override string AssetName
        {
            get
            {
                return "UNET";
            }
        }

        public override string AssetNameSpace
        {
            get
            {
                return "UN_UNet";
            }
        }

        public override string PublisherName
        {
            get
            {
                return "Unity Technologies";
            }
        }

        public override string AssetDocumentationName
        {
            get
            {
                return "UN_UNET.pdf";
            }
        }

        [MethodHelper()]
        public void CreateManager()
        {
            #if UN_UNet
            var instance = GameObject.FindObjectOfType<uNature.Extensions.UNet.UNetCallbackManager>();

            if(instance == null)
            {
                GameObject go = new GameObject("UN Networking Manager");
                go.AddComponent<uNature.Extensions.UNet.UNetCallbackManager>();
            }
            #endif
        }

    }
}