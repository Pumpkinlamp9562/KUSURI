#if UN_PhotonBolt

using UnityEngine;
using System.Collections;

namespace uNature.Extensions.PhotonBolt
{
    public class Player
    {
        public BoltEntity entity;
        public BoltConnection cn;

        public Player()
        {
            entity = null;
            cn = null;
        }

        public Player(BoltConnection cn)
        {
            entity = BoltNetwork.Instantiate(BoltPrefabs.Player, new Vector3(1000f, 119.93f, 1082.5f), Quaternion.identity);

            this.cn = cn;

            if (cn == null)
            {
                entity.TakeControl();
            }
            else
            {
                entity.AssignControl(cn);
            }

        }
    }
}

#endif
