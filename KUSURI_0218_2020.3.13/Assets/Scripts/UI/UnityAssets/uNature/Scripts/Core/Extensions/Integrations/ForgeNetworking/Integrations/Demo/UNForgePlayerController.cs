#if UN_ForgeNetworking

using UnityEngine;
using System.Collections;

using BeardedManStudios.Network;

using uNature.Core.Networking;

namespace uNature.Extensions.ForgeNetworking
{
    public class UNForgePlayerController : UNNetworkPlayerController
    {
        NetworkedMonoBehavior _networkedMonoBehaviour;
        NetworkedMonoBehavior networkedMonoBehaviour
        {
            get
            {
                if(_networkedMonoBehaviour == null)
                {
                    _networkedMonoBehaviour = GetComponent<NetworkedMonoBehavior>();
                }

                return _networkedMonoBehaviour;
            }
        }

        protected override bool hasControl
        {
            get
            {
                return networkedMonoBehaviour.IsOwner;
            }
        }

        public void Start()
        {
            base.OnAttached();
        }
    }
}

#endif