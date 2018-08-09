using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePositioner : MonoBehaviour
{

	[SerializeField] private Vector3 FirstLinePos;
	[SerializeField] private Vector3 LineInc;

	void Start()
	{
		GoToLine(3);
	}

	public void GoToLine(int line)
	{
		transform.position = FirstLinePos + (line - 1) * LineInc;
	}
}
