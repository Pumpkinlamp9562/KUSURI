#if UN_PhotonCloud

using UnityEngine;
using System.Collections;

namespace uNature.Extensions.PhotonCloud
{
    public class PlayerInstantiater : Photon.PunBehaviour
    {
        public override void OnJoinedRoom()
        {
            PhotonNetwork.Instantiate("Player/PhotonPlayer", new Vector3(1000f, 119.93f, 1082.5f), Quaternion.identity, 0);
        }
    }
}

#endif