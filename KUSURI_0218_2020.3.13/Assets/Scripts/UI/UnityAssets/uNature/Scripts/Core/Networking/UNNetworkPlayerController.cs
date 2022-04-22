using UnityEngine;
using System.Collections;

namespace uNature.Core.Networking
{
    public abstract class UNNetworkPlayerController
#if UN_PhotonBolt
 : Bolt.EntityBehaviour<IPlayerState>
#else
 : MonoBehaviour
#endif
    {
        protected virtual bool hasControl
        {
            get { return true; }
        }

        public MonoBehaviour[] disableOnProxies;
        public Camera Camera;
        public CharacterController controller;

        protected virtual void Awake()
        {
            ManageEnableOnProxies(false);

            if (hasControl)
            {
                ManageEnableOnProxies(true);
            }
        }

        public virtual void OnAttached()
        {
            if (hasControl)
            {
                ManageEnableOnProxies(true);
            }
        }

        public void ManageEnableOnProxies(bool value)
        {
            for (int i = 0; i < disableOnProxies.Length; i++)
            {
                disableOnProxies[i].enabled = value;
            }

            Camera.gameObject.SetActive(value);

            if (controller != null)
                controller.enabled = value;
        }
    }
}
