#if UN_PhotonBolt

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

namespace uNature.Extensions.PhotonBolt
{
    public class PhotonBoltNetworkManager : UNNetworkManager<BoltConnection, PhotonBoltNetworkData>
    {
        public static UdpKit.UdpChannelName streamChannel;

        public override bool isServer
        {
            get
            {
                return BoltNetwork.isServer;
            }
        }

        public PhotonBoltNetworkManager(MonoBehaviour managerInstance) : base(managerInstance)  { }
    }
}

#endif