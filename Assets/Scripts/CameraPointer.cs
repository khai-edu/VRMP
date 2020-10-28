//-----------------------------------------------------------------------
// <copyright file="CameraPointer.cs" company="Google LLC">
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

using System.Collections;
using UnityEngine;

/// <summary>
/// Sends messages to gazed GameObject.
/// </summary>
public class CameraPointer : MonoBehaviour
{
	public GameObject Player = null;

	private const float _maxDistance = 10;
	private GameObject _gazedAtObject = null;
	private const float _timeToSelect = 3.0f;
	private const string _interactableObjectTag = "InteractableObject";

	private float _timeLeft = 0.0f;

	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	public void Update()
	{
		// Casts ray towards camera's forward direction, to detect if a GameObject is being gazed
		// at.
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, _maxDistance) && hit.transform.gameObject.tag == _interactableObjectTag)
		{
			// GameObject detected in front of the camera.
			if (_gazedAtObject != hit.transform.gameObject)
			{
				// New GameObject.
				_gazedAtObject?.SendMessage("OnPointerExit", SendMessageOptions.DontRequireReceiver);
				_gazedAtObject = hit.transform.gameObject;
				_gazedAtObject.SendMessage("OnPointerEnter", SendMessageOptions.DontRequireReceiver);
				StartTimer(_timeToSelect);
			}
		}
		else
		{
			// No GameObject detected in front of the camera.
			_gazedAtObject?.SendMessage("OnPointerExit", SendMessageOptions.DontRequireReceiver);
			_gazedAtObject = null;
			StopTimer();
			UIManager.Instance.SetSlectorAmount(0);
		}

		// Checks for screen touches.
		if (Google.XR.Cardboard.Api.IsTriggerPressed)
		{
			_gazedAtObject?.SendMessage("OnPointerClick", Player, SendMessageOptions.DontRequireReceiver);
		}

		UpdateTimer();
	}

	private void StartTimer(float seconds)
	{
		_timeLeft = seconds;
		UIManager.Instance.ShowSelector(true);
		UIManager.Instance.SetSlectorAmount(1.0f);
	}

	private void StopTimer()
	{
		_timeLeft = 0.0f;
		UIManager.Instance.ShowSelector(false);
	}

	private void OnTimerDone()
	{
		_gazedAtObject?.SendMessage("OnPointerClick", Player, SendMessageOptions.DontRequireReceiver);
		StopTimer();
	}

	private void UpdateTimer()
	{
		if(_timeLeft > 0)
		{
			_timeLeft -= Time.deltaTime;
			if(_timeLeft <= 0)
			{
				OnTimerDone();
			}
			UIManager.Instance.SetSlectorAmount(1 - (_timeLeft / _timeToSelect));
		}
	}
}
