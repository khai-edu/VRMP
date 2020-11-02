using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{

	/// <summary>
	/// Returns the full hierarchy name of the game object.
	/// </summary>
	/// <param name="go">The game object.</param>
	public static string GetFullName(this GameObject go)
	{
		string name = go.name;
		while (go.transform.parent != null)
		{

			go = go.transform.parent.gameObject;
			name = go.name + "/" + name;
		}
		return name;
	}

	/// <summary>
	/// Returns the full hierarchy name of the tranform.
	/// </summary>
	/// <param name="go">The game object.</param>
	public static string GetFullName(this Transform transform)
	{
		string name = transform.name;
		while (transform.parent != null)
		{

			transform = transform.parent;
			name = transform.name + "/" + name;
		}
		return name;
	}
}
