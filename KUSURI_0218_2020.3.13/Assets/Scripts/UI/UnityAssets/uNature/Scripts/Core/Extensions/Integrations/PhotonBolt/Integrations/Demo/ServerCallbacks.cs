#if UN_PhotonBolt

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace uNature.Extensions.PhotonBolt
{
    [BoltGlobalBehaviour(BoltNetworkModes.Host)]
    public class ServerCallbacks : Bolt.GlobalEventListener
    {
        List<Player> players = new List<Player>();

        public override void SceneLoadLocalDone(string map)
        {
            players.Add(new Player(null));
        }

        public override void SceneLoadRemoteDone(BoltConnection connection)
        {
            players.Add(new Player(connection));
        }

        public Player Player(BoltConnection cn)
        {
            return players.FirstOrDefault(x => x.cn == cn);
        }

        public Player Player(BoltEntity entity)
        {
            return players.FirstOrDefault(x => x.entity.Equals(entity));
        }
    }
}
#endif