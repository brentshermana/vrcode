using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorAnim : MonoBehaviour
{

	[SerializeField] private Vector3 movement_axis;
	[SerializeField] private float period;
	[SerializeField] private float delta;

	private Vector3 start_pos;
	private float start_time;

	// Use this for initialization
	void Start ()
	{
		start_pos = transform.position;
		start_time = Time.unscaledTime;
		movement_axis = movement_axis.normalized;
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.position = start_pos + movement_axis * delta * Mathf.Sin((Time.unscaledTime - start_time) * 2f*Mathf.PI / period);
	}
}
