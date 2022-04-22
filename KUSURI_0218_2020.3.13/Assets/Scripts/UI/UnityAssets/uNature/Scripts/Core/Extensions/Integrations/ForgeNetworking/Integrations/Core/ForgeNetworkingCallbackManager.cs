#if UN_ForgeNetworking
using UnityEngine;
using System.Collections;

using uNature.Core.Networking;
using BeardedManStudios.Network;

namespace uNature.Extensions.ForgeNetworking
{
    public class ForgeNetworkingCallbackManager : MonoBehaviour
    {
        ForgeNetworkingNetworkManager networkingManager;

        void OnEnable()
        {
            Networking.PrimarySocket.AddCustomDataReadEvent(ForgeNetworkingNetworkData.id, networkingManager.ReceiveTreeInstancesStream);
        }

        void Awake()
        {
            networkingManager = new ForgeNetworkingNetworkManager(this);

            if (!Networking.PrimarySocket.Connected)
            {
                Networking.connected += (NetWorker socket) =>
                    {
                        Setup();
                    };
            }
            else
            {
                Setup();
            }
        }

        void Setup()
        {
            Networking.PrimarySocket.playerConnected += (NetworkingPlayer player) =>
            {
                networkingManager.OnClientConnected(player);
            };

            Networking.ClientReady(Networking.PrimarySocket);
        }

    }
}

#endif