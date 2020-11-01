using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonParentView : MonoBehaviourPun, IPunObservable
{
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			string parentName = this.gameObject.transform.parent.name;
			stream.SendNext(parentName);
		}
		else
		{
			string parentName = (string)stream.ReceiveNext();
			var parent = GameObject.Find(parentName);
			this.gameObject.transform.SetParent(parent.transform);
		}
	}
}
