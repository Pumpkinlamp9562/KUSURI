#if UN_PhotonCloud

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

namespace uNature.Extensions.PhotonCloud
{
    public class PhotonCloudNetworkManager : UNNetworkManager<PhotonPlayer, PhotonCloudNetworkData>
    {
        public override bool isServer
        {
            get
            {
                return PhotonNetwork.isMasterClient;
            }
        }

        public override void Awake()
        {
            base.Awake();

            if (PhotonNetwork.connected)
            {
                PhotonNetwork.OnEventCall += OnEventCalled;
            }
        }

        public PhotonCloudNetworkManager(MonoBehaviour managerInstance) : base(managerInstance)  { }

        /// <summary>
        /// Called when an event is called
        /// </summary>
        /// <param name="eventCode"></param>
        /// <param name="content"></param>
        /// <param name="senderId"></param>
        protected virtual void OnEventCalled(byte eventCode, object content, int senderId)
        {
            if(eventCode == PhotonCloudNetworkData.eventCode)
            {
                var data = PhotonCloudNetworkData.Deserialize((byte[])content);

                data.UnPack();
            }
        }
    }
}

#endif