using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;

[RequireComponent(typeof(PhotonView))]
public class PhotonConfig : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
	[SerializeField]
	private GameObject Camera;

	[SerializeField]
	private GameObject[] HideGameObjectsForMe;

	[SerializeField]
	private GameObject[] HideGameObjectsForYou;

	[SerializeField]
	private MonoBehaviour[] DisableMonoBegaviurForMe;

	[SerializeField]
	private MonoBehaviour[] DisableMonoBegaviurForYou;

	private bool IsKinematicDefault = false;

	private void Awake()
	{
		if (TryGetComponent(out Rigidbody rigidbody))
		{
			IsKinematicDefault = rigidbody.isKinematic;
		}

		ConfigureComponents();
	}

	void ConfigureComponents()
	{
		if(photonView == null)
		{
			Debug.LogError("PhotonView is null!");
			return;
		}

		bool isLocalClient = photonView.IsMine || !PhotonNetwork.InRoom;

		if (Camera != null)
		{
			if(Camera.TryGetComponent(out Camera cameraRef))
			{
				cameraRef.enabled = isLocalClient;
			}

			if (Camera.TryGetComponent(out AudioListener audioListenerRef))
			{
				audioListenerRef.enabled = isLocalClient;
			}

			if (Camera.TryGetComponent(out TrackedPoseDriver trackedPoseDriver))
			{
				trackedPoseDriver.enabled = isLocalClient;	
			}
		}

		if (TryGetComponent(out Rigidbody rigidbody))
		{
			if (!isLocalClient)
			{
				rigidbody.isKinematic = true;
			}
			else
			{
				rigidbody.isKinematic = IsKinematicDefault;
			}
		}

		foreach(GameObject obg in HideGameObjectsForMe)
		{
			if(obg != null && obg.TryGetComponent(out MeshRenderer mesh))
			{
				mesh.enabled = !isLocalClient;
			}
		}

		foreach (GameObject obg in HideGameObjectsForYou)
		{
			if (obg != null && obg.TryGetComponent(out MeshRenderer mesh))
			{
				mesh.enabled = isLocalClient;
			}
		}

		foreach (MonoBehaviour beh in DisableMonoBegaviurForMe)
		{
			beh.enabled = !isLocalClient;
		}

		foreach (MonoBehaviour beh in DisableMonoBegaviurForYou)
		{
			beh.enabled = isLocalClient;
		}
	}

	public void ChangeOwnershipOnMe()
	{
		if (photonView == null)
		{
			Debug.LogError("PhotonView is null!");
			return;
		}

		if (photonView.OwnershipTransfer == OwnershipOption.Takeover)
		{
			photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
		}
		else
		{
			Debug.LogError("Impossible to transfer ownership!");
		}
	}

	public override void OnJoinedRoom()
	{
		ConfigureComponents();
	}

	public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
	{
	}

	public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
	{
		if(targetView.ViewID == photonView.ViewID)
		{
			ConfigureComponents();
		}
	}
}
