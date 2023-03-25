using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public partial class Extension
{
	public static Transform DeepFind(this Transform gameObject, string name)
	{
		foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
		{
			if (child.name == name)
				return child;
		}

		return null;
	}
}
