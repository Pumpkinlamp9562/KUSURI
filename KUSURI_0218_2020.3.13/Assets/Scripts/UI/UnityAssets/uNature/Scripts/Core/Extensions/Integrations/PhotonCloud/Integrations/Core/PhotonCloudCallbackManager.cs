#if UN_PhotonCloud

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

namespace uNature.Extensions.PhotonCloud
{
    public class PhotonCloudCallbackManager : Photon.PunBehaviour
    {
        PhotonCloudNetworkManager networkingManager;

        void Awake()
        {
            networkingManager = new PhotonCloudNetworkManager(this);
        }

        /// <summary>
        /// Called when joined to a room
        /// </summary>
        public override void OnJoinedRoom()
        {
            networkingManager.Awake();
        }

        /// <summary>
        /// Called when ownership is witched, update permissions
        /// </summary>
        /// <param name="newMasterClient"></param>
        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            networkingManager.UpdatePermissions();
        }

        /// <summary>
        /// Called when a new player connects
        /// </summary>
        /// <param name="newPlayer">the new player</param>
        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            networkingManager.OnClientConnected(newPlayer);
        }

    }
}

#endif