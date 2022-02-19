#if UN_ForgeNetworking

using UnityEngine;
using System.Collections;

using BeardedManStudios.Forge;
using BeardedManStudios.Network;

namespace uNature.Extensions.ForgeNetworking
{
    public class PlayerInstantiater : MonoBehaviour
    {
        public GameObject Player;

        void Start()
        {
            if (Player == null)
            {
                Debug.LogError("Player not assigned to PlayerInstantiater!!");
                this.enabled = false;

                return;
            }

            if (NetworkingManager.Socket == null || NetworkingManager.Socket.Connected)
                Networking.Instantiate(Player, new Vector3(1000f, 119.93f, 1082.5f), Quaternion.identity, NetworkReceivers.AllBuffered);
            else
            {
                NetworkingManager.Instance.OwningNetWorker.connected += delegate()
                {
                    Networking.Instantiate(Player, new Vector3(1000f, 119.93f, 1082.5f), Quaternion.identity, NetworkReceivers.AllBuffered);
                };
            }
        }
    }
}

#endif
