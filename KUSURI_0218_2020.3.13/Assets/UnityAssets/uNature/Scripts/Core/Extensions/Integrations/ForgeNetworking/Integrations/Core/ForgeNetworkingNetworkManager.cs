#if UN_ForgeNetworking

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

using BeardedManStudios.Network;

namespace uNature.Extensions.ForgeNetworking
{
    public class ForgeNetworkingNetworkManager : UNNetworkManager<NetworkingPlayer, ForgeNetworkingNetworkData>
    {
        public override bool isServer
        {
            get
            {
                return Networking.PrimarySocket.IsServer;
            }
        }

        public virtual void ReceiveTreeInstancesStream(NetworkingPlayer player, NetworkingStream stream)
        {
            BeardedManStudios.Network.Unity.MainThreadManager.Run(() =>
                {
                    var data = ForgeNetworkingNetworkData.Deserialize(stream);
                    data.UnPack();
                });
        }

        public ForgeNetworkingNetworkManager(MonoBehaviour managerInstance) : base(managerInstance)  { }
    }
}

#endif