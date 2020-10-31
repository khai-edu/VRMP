using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;

[RequireComponent(typeof(PhotonView))]
public class PhotonConfigureCamera : MonoBehaviour
{
	[SerializeField]
	private GameObject Camera;

	[SerializeField]
	private GameObject[] HideGameObjectsForMe;

	private PhotonView photonView;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();

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

		foreach(GameObject obg in HideGameObjectsForMe)
		{
			if(obg != null && obg.TryGetComponent(out MeshRenderer mesh))
			{
				mesh.enabled = !isLocalClient;
			}
		}
	}
}
