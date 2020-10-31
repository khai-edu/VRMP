using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
	[SerializeField]
	private Transform SpawnPoint = null;

	[SerializeField]
	private GameObject PrefabCardboard = null;

	[SerializeField]
	private GameObject PrefabMixedReality = null;

#if UNITY_EDITOR
	enum DebugMode
	{
		None,
		Cardboard,
		MixedReality,
	}

	[SerializeField]
	private DebugMode ForceDebugMode = DebugMode.None;
#endif

	void Start()
	{
		if (NetworkManager.Instance == null)
		{
			Spawn();
		}
	}

	public override void OnJoinedRoom()
	{
		Spawn();
	}

	void Spawn()
	{
		if (SpawnPoint == null)
		{
			Debug.LogError("SpawnPoint is null!");
		}

		RuntimePlatform currentPlatfrom = Application.platform;

#if UNITY_EDITOR
		switch(ForceDebugMode)
		{
			case DebugMode.Cardboard:
				currentPlatfrom = RuntimePlatform.Android;
				break;
			case DebugMode.MixedReality:
				currentPlatfrom = RuntimePlatform.WindowsPlayer;
				break;
			default:
				break;
		}
#endif

		GameObject gameobject;
		if (currentPlatfrom == RuntimePlatform.Android)
		{
			gameobject = PrefabCardboard;
		}
		else
		{
			gameobject = PrefabMixedReality;
		}

		if (gameobject != null)
		{
			if (PhotonNetwork.InRoom)
			{
				PhotonNetwork.Instantiate(gameobject.name, SpawnPoint.position, Quaternion.identity);
			}
			else
			{
				Instantiate(gameobject, SpawnPoint.position, Quaternion.identity);
			}
		}
		else
		{
			Debug.LogError("No one to spawn!");
		}
	}
}
