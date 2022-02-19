#if UN_PhotonCloud

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

namespace uNature.Extensions.PhotonCloud
{
    [System.Serializable]
    public class PhotonCloudNetworkData : UNNetworkData<PhotonPlayer>
    {
        public const byte eventCode = 122;

        public override void SendToClients()
        {
            var eventOptions = RaiseEventOptions.Default;
            eventOptions.Receivers = ReceiverGroup.Others;

            PhotonNetwork.RaiseEvent(eventCode, Serialize(), true, eventOptions);
        }

        public override void SendToConnection(PhotonPlayer connection)
        {
            var eventOptions = RaiseEventOptions.Default;
            eventOptions.TargetActors = new int[1] { connection.ID };

            PhotonNetwork.RaiseEvent(eventCode, Serialize(), true, eventOptions);
        }

        public override void SendToOthers()
        {
            var eventOptions = RaiseEventOptions.Default;
            eventOptions.Receivers = ReceiverGroup.Others;

            PhotonNetwork.RaiseEvent(eventCode, Serialize(), true, eventOptions);
        }

        public override void SendToServer()
        {
            var eventOptions = RaiseEventOptions.Default;
            eventOptions.Receivers = ReceiverGroup.Others;

            PhotonNetwork.RaiseEvent(eventCode, Serialize(), true, eventOptions);
        }
    }
}

#endif