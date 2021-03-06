//-----------------------------------------------------------------------
// <copyright file="CardboardStartup.cs" company="Google LLC">
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using Google.XR.Cardboard;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Initializes Cardboard XR Plugin.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class CardboardStartup : MonoBehaviour
{
	private PhotonView photonView;

	[SerializeField]
	public GameObject Selector = null;

	private void Awake()
	{
		photonView = GetComponent<PhotonView>();
	}

	void ConfigureCardboards()
	{
		if (photonView != null)
		{
			if (photonView.IsMine || !PhotonNetwork.InRoom)
			{
				Debug.Log("Crated Cardboard by mine");
				// Configures the app to not shut down the screen and sets the brightness to maximum.
				// Brightness control is expected to work only in iOS, see:
				// https://docs.unity3d.com/ScriptReference/Screen-brightness.html.
				Screen.sleepTimeout = SleepTimeout.NeverSleep;
				Screen.brightness = 1.0f;

				// Checks if the device parameters are stored and scans them if not.
				if (!Api.HasDeviceParams())
				{
					Api.ScanDeviceParams();
					QualitySettings.vSyncCount = 1;
				}
			}
		}
	}

	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	public void Update()
	{
		if (photonView != null && photonView.IsMine)
		{
			if (Api.IsGearButtonPressed)
			{
				Api.ScanDeviceParams();
			}

			if (Api.IsCloseButtonPressed)
			{
				Application.Quit();
			}

			if (Api.HasNewDeviceParams())
			{
				Api.ReloadDeviceParams();
			}

			if(Selector != null)
			{
				UIManager.Instance.Selector = Selector;
			}
		}
	}
}
