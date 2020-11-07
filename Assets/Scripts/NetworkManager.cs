using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	[SerializeField]
	private string gameVersion = "1.0.0";

	public static NetworkManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this.gameObject);

			PhotonNetwork.AutomaticallySyncScene = true;
			PhotonNetwork.PrecisionForFloatSynchronization = 0.000099f;
		}
		else
		{
			Debug.LogWarning("There should only be one manager");
			Destroy(this.gameObject);
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		Connect();
	}

	// Update is called once per frame
	void Update()
	{
	}

	public void Connect()
	{
		// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
		if (PhotonNetwork.IsConnected)
		{
			JoinToRoom();
		}
		else
		{
			// #Critical, we must first and foremost connect to Photon Online Server.
			PhotonNetwork.GameVersion = gameVersion;
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	#region MonoBehaviourPunCallbacks Callbacks
	public override void OnConnectedToMaster()
	{
		Debug.Log("OnConnectedToMaster() was called by PUN");
		JoinToRoom();
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.LogFormat("OnDisconnected() was called by PUN with reason {0}", cause);
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Debug.LogErrorFormat("Room creation failed with error code {0} and error message {1}", returnCode, message);
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("OnJoinedRoom() was called by PUN");
	}
	#endregion

	private void JoinToRoom()
	{
		RoomOptions options = new RoomOptions();
		PhotonNetwork.JoinOrCreateRoom("Main", options, null);
	}
}