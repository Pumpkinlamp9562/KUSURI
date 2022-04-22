#if UN_PhotonBolt

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

using System.IO;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;

namespace uNature.Extensions.PhotonBolt
{
    [BoltGlobalBehaviour]
    public class PhotonBoltCallbackManager : Bolt.GlobalEventListener
    {
        PhotonBoltNetworkManager networkingManager;

        void Awake()
        {
            networkingManager = new PhotonBoltNetworkManager(this);
        }

        public override void BoltStartBegin()
        {
            PhotonBoltNetworkManager.streamChannel = BoltNetwork.CreateStreamChannel("UN_Bolt_StreamChannel", UdpKit.UdpChannelMode.Reliable, 1);
        }

        public override void StreamDataReceived(BoltConnection connection, UdpKit.UdpStreamData data)
        {
            if (data.Channel.Equals(PhotonBoltNetworkManager.streamChannel))
            {
                PhotonBoltNetworkData.Deserialize(data.Data).UnPack();
            }
        }

        /// <summary>
        /// Called when a new player connects
        /// </summary>
        /// <param name="connection">the new player</param>
        public override void SceneLoadRemoteDone(BoltConnection connection)
        {
            if (BoltNetwork.isServer)
            {
                networkingManager.OnClientConnected(connection);
            }
        }

    }
}

#endif