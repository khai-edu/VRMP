using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;

[RequireComponent(typeof(PhotonView))]
public class PhotonConfig : MonoBehaviour
{
	[SerializeField]
	private GameObject Camera;

	[SerializeField]
	private GameObject[] HideGameObjectsForMe;

	[SerializeField]
	private GameObject[] HideGameObjectsForYou;

	private bool IsKinematicDefault = false;

	private PhotonView photonView;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();

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
	}
}
