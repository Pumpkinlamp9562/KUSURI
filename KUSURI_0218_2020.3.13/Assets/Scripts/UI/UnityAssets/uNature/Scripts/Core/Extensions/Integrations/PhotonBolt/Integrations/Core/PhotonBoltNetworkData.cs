#if UN_PhotonBolt

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;

using System.IO;

namespace uNature.Extensions.PhotonBolt
{
    [System.Serializable]
    public class PhotonBoltNetworkData : UNNetworkData<BoltConnection>
    {
        public override void SendToClients()
        {
            foreach (var connection in BoltNetwork.connections)
            {
                connection.StreamBytes(PhotonBoltNetworkManager.streamChannel, Serialize());
            }
        }

        public override void SendToConnection(BoltConnection connection)
        {
            connection.StreamBytes(PhotonBoltNetworkManager.streamChannel, Serialize());
        }

        public override void SendToOthers()
        {
            foreach (var connection in BoltNetwork.connections)
            {
                connection.StreamBytes(PhotonBoltNetworkManager.streamChannel, Serialize());
            }
        }

        public override void SendToServer()
        {
            BoltNetwork.server.StreamBytes(PhotonBoltNetworkManager.streamChannel, Serialize());
        }
    }
}

#endif