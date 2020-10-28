using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VREmulator : MonoBehaviour
{
#if UNITY_EDITOR
	private const string AXIS_MOUSE_X = "Mouse X";
	private const string AXIS_MOUSE_Y = "Mouse Y";

	// Simulated neck model.  Vector from the neck pivot point to the point between the eyes.
	private static readonly Vector3 NECK_OFFSET = new Vector3(0, 0.075f, 0.08f);

	// Allocate an initial capacity; this will be resized if needed.
	private static Camera[] allCameras = new Camera[32];

	// Use mouse to emulate head in the editor.
	// These variables must be static so that head pose is maintained between scene changes,
	// as it is on device.
	private float mouseX = 0;
	private float mouseY = 0;
	private float mouseZ = 0;

	/// <summary>Gets the emulated head position.</summary>
	/// <value>The emulated head position.</value>
	public Vector3 HeadPosition { get; private set; }

	/// <summary>Gets the emulated head rotation.</summary>
	/// <value>The emulated head rotation.</value>
	public Quaternion HeadRotation { get; private set; }

	/// <summary>Recenters the emulated headset.</summary>
	public void Recenter()
	{
		mouseX = mouseZ = 0;  // Do not reset pitch, which is how it works on the phone.
		UpdateHeadPositionAndRotation();
		ApplyHeadOrientationToVRCameras();
	}

	/// <summary>Single-frame updates for this module.</summary>
	/// <remarks>Should be called in one MonoBehavior's `Update` method.</remarks>
	public void UpdateEditorEmulation()
	{
		bool rolled = false;
		if (CanChangeYawPitch())
		{
			mouseX += Input.GetAxis(AXIS_MOUSE_X) * 5;
			if (mouseX <= -180)
			{
				mouseX += 360;
			}
			else if (mouseX > 180)
			{
				mouseX -= 360;
			}

			mouseY -= Input.GetAxis(AXIS_MOUSE_Y) * 2.4f;
			mouseY = Mathf.Clamp(mouseY, -85, 85);
		}
		else if (CanChangeRoll())
		{
			rolled = true;
			mouseZ += Input.GetAxis(AXIS_MOUSE_X) * 5;
			mouseZ = Mathf.Clamp(mouseZ, -85, 85);
		}

		if (!rolled)
		{
			// People don't usually leave their heads tilted to one side for long.
			mouseZ = Mathf.Lerp(mouseZ, 0, Time.deltaTime / (Time.deltaTime + 0.1f));
		}

		UpdateHeadPositionAndRotation();
		ApplyHeadOrientationToVRCameras();
	}

	private void Awake()
	{
	}

	private void Start()
	{
		UpdateAllCameras();
	}

	private void Update()
	{
		UpdateEditorEmulation();
	}

	private bool CanChangeYawPitch()
	{
		return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
	}

	private bool CanChangeRoll()
	{
		return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
	}

	private void UpdateHeadPositionAndRotation()
	{
		HeadRotation = Quaternion.Euler(mouseY, mouseX, mouseZ);
		HeadPosition = (HeadRotation * NECK_OFFSET) - (NECK_OFFSET.y * Vector3.up);
	}

	private void ApplyHeadOrientationToVRCameras()
	{
		UpdateAllCameras();

		// Update all VR cameras using Head position and rotation information.
		for (int i = 0; i < Camera.allCamerasCount; ++i)
		{
			Camera cam = allCameras[i];

			// Check if the Camera is a valid VR Camera, and if so update it to track head motion.
			if (cam && cam.enabled && cam.stereoTargetEye != StereoTargetEyeMask.None)
			{
				cam.transform.localPosition = HeadPosition * cam.transform.lossyScale.y;
				cam.transform.localRotation = HeadRotation;
			}
		}
	}

	// Avoids per-frame allocations. Allocates only when allCameras array is resized.
	private void UpdateAllCameras()
	{
		// Get all Cameras in the scene using persistent data structures.
		if (Camera.allCamerasCount > allCameras.Length)
		{
			int newAllCamerasSize = Camera.allCamerasCount;
			while (Camera.allCamerasCount > newAllCamerasSize)
			{
				newAllCamerasSize *= 2;
			}

			allCameras = new Camera[newAllCamerasSize];
		}

		// The GetAllCameras method doesn't allocate memory (Camera.allCameras does).
		Camera.GetAllCameras(allCameras);
	}

#endif  // UNITY_EDITOR
}
