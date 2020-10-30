using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
	[SerializeField]
	private Transform SpawnPoint = null;

	[SerializeField]
	private GameObject PrefabCardboard;

	[SerializeField]
	private GameObject PrefabMixedReality;

	[SerializeField]

	void Awake()
	{
		if (SpawnPoint == null)
		{
			Debug.LogError("SpawnPoint is null!");
		}

		GameObject gameobject = null;
		if(Application.platform == RuntimePlatform.Android)
		{
			gameobject = PrefabCardboard;
		}
		else
		{
			gameobject = PrefabMixedReality;
		}

		if (gameobject != null)
		{ 
			Instantiate(gameobject, SpawnPoint.position, Quaternion.identity);
		}
		else
		{
			Debug.LogError("No one to spawn!");
		}
	}
}
