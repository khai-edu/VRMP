using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class mamalehi : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPhotonSerializeView(PhotonStream _stream, PhotonMessageInfo _info)
    {
        var vp = GetComponent<UnityEngine.Video.VideoPlayer>();
        if (_stream.IsWriting)
        {
            
            _stream.SendNext(vp.time);
        }

        else
        {
            vp.time = (double)_stream.ReceiveNext();
        }
    }
}
