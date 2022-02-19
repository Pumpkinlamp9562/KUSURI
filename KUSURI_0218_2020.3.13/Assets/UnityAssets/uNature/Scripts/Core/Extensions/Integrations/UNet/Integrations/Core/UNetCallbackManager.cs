#if UN_UNet

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

using System.IO;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;

using UnityEngine.Networking;

namespace uNature.Extensions.UNet
{
    public class UNetCallbackManager : NetworkManager
    {
        UNetNetworkManager networkingManager;

        /// <summary>
        /// Initiate instance
        /// </summary>
        public virtual void Start()
        {
            networkingManager = new UNetNetworkManager(this);
        }

        /// <summary>
        /// Set up callbacks
        /// </summary>
        /// <param name="conn">our socket</param>
        public override void OnStartClient(NetworkClient client)
        {
            if (NetworkServer.active)
            {
                NetworkServer.RegisterHandler(UNetNetworkData.MSG, ReceiveEvent);
            }
            else
            {
                NetworkClient.allClients[0].RegisterHandler(UNetNetworkData.MSG, ReceiveEvent);
            }

            UNetNetworkManager.manager.UpdatePermissions();
        }

        protected void ReceiveEvent(NetworkMessage msg)
        {
            var data = msg.ReadMessage<UNetNetworkData>();
            data.UnPack();
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);

            if (NetworkServer.active)
            {
                networkingManager.OnClientConnected(conn);
            }
        }

    }
}

#endif