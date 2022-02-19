#if UN_UNet

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;
using UnityEngine.Networking;

namespace uNature.Extensions.UNet
{
    public class UNetNetworkManager : UNNetworkManager<NetworkConnection, UNetNetworkData>
    {
        public override bool isServer
        {
            get
            {
                return NetworkServer.active;
            }
        }

        public override void Awake()
        {
            base.Awake();

            manager = this;
        }

        public UNetNetworkManager(MonoBehaviour managerInstance) : base(managerInstance)  { }
    }
}

#endif