#if UN_ForgeNetworking

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

using BeardedManStudios.Network;

namespace uNature.Extensions.ForgeNetworking
{
    [System.Serializable]
    public class ForgeNetworkingNetworkData : UNNetworkData<NetworkingPlayer>
    {
        public static uint id = 125425;

        private BMSByte data = new BMSByte();

        public override void SendToClients()
        {
            Networking.WriteCustom(id, Networking.PrimarySocket, data.Clone(Serialize()), true, NetworkReceivers.Others);
        }

        public override void SendToConnection(NetworkingPlayer connection)
        {
            Networking.WriteCustom(id, Networking.PrimarySocket, data.Clone(Serialize()), connection, true);
        }

        public override void SendToOthers()
        {
            Networking.WriteCustom(id, Networking.PrimarySocket, data.Clone(Serialize()), true, NetworkReceivers.Others);
        }

        public override void SendToServer()
        {
            Networking.WriteCustom(id, Networking.PrimarySocket, data.Clone(Serialize()), true, NetworkReceivers.Server);
        }

        BMSByte Serialize()
        {
            data = new BMSByte();

            ObjectMapper.MapBytes(data, terrainID, treeInstanceID, _health, eventType);
            return data;
        }

        public static ForgeNetworkingNetworkData Deserialize(NetworkingStream stream)
        {
            ForgeNetworkingNetworkData instance = new ForgeNetworkingNetworkData();

            instance.terrainID = ObjectMapper.Map<string>(stream);
            instance.treeInstanceID = ObjectMapper.Map<int>(stream);
            instance._health = ObjectMapper.Map<int>(stream);
            instance.eventType = ObjectMapper.Map<PacketType>(stream);

            return instance;
        }
    }
}

#endif