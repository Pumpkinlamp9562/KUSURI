#if UN_UNet

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;
using UnityEngine.Networking;

namespace uNature.Extensions.UNet
{
    public class UNetPlayerController : UNNetworkPlayerController
    {
        public static Vector3 spawnPoint = new Vector3(1000f, 119.93f, 1082.5f);

        NetworkIdentity _entity;
        NetworkIdentity entity
        {
            get
            {
                if(_entity == null)
                {
                    _entity = GetComponent<NetworkIdentity>();
                }

                return _entity;
            }
        }

        protected override bool hasControl
        {
            get
            {
                return entity.hasAuthority;
            }
        }

        public virtual void Start()
        {
            OnAttached();
            this.transform.position = spawnPoint;
        }
    }
}

#endif