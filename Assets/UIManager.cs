using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; set; }

	[SerializeField]
	GameObject Selector = null;

	void Awake()
	{
		if(Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("There should be only one UIManager!");
		}
		ShowSelector(false);
		SetSlectorAmount(0.0f);
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void ShowSelector(bool value)
	{
		if (Selector != null)
		{
			Selector.SetActive(value);
		}
	}

	public void SetSlectorAmount(float value)
	{
		if (Selector != null)
		{
			var img = Selector.GetComponent<UnityEngine.UI.Image>();
			if (img != null)
			{
				img.fillAmount = value;
			}
			else
			{
				Debug.LogError("The selector has no image!");
			}
		}
	}
}
