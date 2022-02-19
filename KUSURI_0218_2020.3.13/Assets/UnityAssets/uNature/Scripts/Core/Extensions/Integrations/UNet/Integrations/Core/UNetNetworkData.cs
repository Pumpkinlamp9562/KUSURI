#if UN_UNet

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;

using System.IO;

using UnityEngine.Networking;

namespace uNature.Extensions.UNet
{
    [System.Serializable]
    public class UNetNetworkData : UNNetworkData<NetworkConnection>
    {
        public const short MSG = 218;

        public override void SendToClients()
        {
            for (int i = 0; i < NetworkServer.connections.Count; i++)
            {
                NetworkServer.connections[i].Send(MSG, this);
            }
        }

        public override void SendToConnection(NetworkConnection connection)
        {
            connection.Send(MSG, this);
        }

        public override void SendToOthers()
        {
            if (NetworkServer.active)
            {
                NetworkServer.SendToAll(MSG, this);
            }
            else
            {
                for (int i = 0; i < NetworkServer.connections.Count; i++)
                {
                    NetworkServer.connections[i].Send(MSG, this);
                }

                if (!NetworkServer.active)
                {
                    SendToServer();
                }
            }
        }

        public override void SendToServer()
        {
            NetworkClient.allClients[0].Send(MSG, this);
        }

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(terrainID);
            writer.Write(treeInstanceID);
            writer.Write(_health);
            writer.Write((byte)eventType);
        }

        public override void Deserialize(NetworkReader reader)
        {
            this.terrainID = reader.ReadString();
            this.treeInstanceID = reader.ReadInt32();
            this._health = reader.ReadInt32();
            this.eventType = (PacketType)reader.ReadByte();
        }
    }
}

#endif