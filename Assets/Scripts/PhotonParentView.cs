using Photon.Pun;
using UnityEngine;

public class PhotonParentView : MonoBehaviourPun, IPunObservable
{
	static string nullstring = ":null";

	private bool debugStopSpam = false;

	public void OnPhotonSerializeView(PhotonStream _stream, PhotonMessageInfo _info)
	{
		if (_stream.IsWriting)
		{
			Transform _transform = gameObject.transform;
			if (_transform.parent)
			{
				string parentName = _transform.parent.GetFullName();
				_stream.SendNext(parentName);
			}
			else
			{
				_stream.SendNext(nullstring);
			}
		}
		else
		{
			string parentName = (string)_stream.ReceiveNext();
			if (parentName != nullstring)
			{
				var parent = GameObject.Find(parentName);
				if (parent != null)
				{
					debugStopSpam = false;
					gameObject.transform.SetParent(parent.transform);
				}
				else if (!debugStopSpam)
				{
					debugStopSpam = true;
					Debug.LogErrorFormat("Cannot find GameObject: \"{0}\". Scene out of sync?", parentName);
				}
			}
			else
			{
				gameObject.transform.SetParent(null);
			}
		}
	}
}
