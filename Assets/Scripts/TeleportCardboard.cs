using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportCardboard : MonoBehaviour
{
	public Transform TeleportPoint;

	/// <summary>
	/// The material to use when this object is inactive (not being gazed at).
	/// </summary>
	public Material InactiveMaterial;

	/// <summary>
	/// The material to use when this object is active (gazed at).
	/// </summary>
	public Material GazedAtMaterial;

	private Renderer _myRenderer;

	private void Awake()
	{
		_myRenderer = GetComponent<Renderer>();
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	/// <summary>
	/// This method is called by the Main Camera when it starts gazing at this GameObject.
	/// </summary>
	public void OnPointerEnter()
	{
		SetMaterial(true);
	}

	/// <summary>
	/// This method is called by the Main Camera when it is gazing at this GameObject and the screen
	/// is touched.
	/// </summary>
	public void OnPointerClick(GameObject obj)
	{
		if(obj != null)
		{
			obj.transform.position = TeleportPoint.transform.position;
		}
	}

	/// <summary>
	/// This method is called by the Main Camera when it stops gazing at this GameObject.
	/// </summary>
	public void OnPointerExit()
	{
		SetMaterial(false);
	}

	private void SetMaterial(bool gazedAt)
	{
		if (InactiveMaterial != null && GazedAtMaterial != null)
		{
			_myRenderer.material = gazedAt ? GazedAtMaterial : InactiveMaterial;
		}
	}
}
