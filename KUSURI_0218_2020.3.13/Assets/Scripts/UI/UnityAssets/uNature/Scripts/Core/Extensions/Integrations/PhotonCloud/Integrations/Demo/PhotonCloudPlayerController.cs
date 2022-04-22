#if UN_PhotonCloud

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

namespace uNature.Extensions.PhotonCloud
{
    public class PhotonCloudPlayerController : UNNetworkPlayerController
    {
        PhotonView _pView;
        PhotonView pView
        {
            get
            {
                if(_pView == null)
                {
                    _pView = GetComponent<PhotonView>();
                }

                return _pView;
            }
        }

        protected override bool hasControl
        {
            get
            {
                return pView == null ? false : pView.isMine;
            }
        }
    }
}

#endif