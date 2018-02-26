using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;
using vrcode.vr.vrui.panel;

namespace vrcode.vr.vrui
{
	[ExecuteInEditMode]
	public class VRUI : MonoBehaviour {

		[SerializeField]
		private Vector3 centerAxis = Vector3.up;

	
		#region IN_EDITOR
		private void OnGUI()
		{
			if (Application.isEditor)
			{
				EditorUpdate();
			}
		}
		void OnRenderObject()
		{
			if (Application.isEditor)
			{
				EditorUpdate();
			}
		}
		void EditorUpdate()
		{
			if (transform.childCount < 1)
			{
				AddPanel(transform.position + Vector3.forward, 2f, 1f);
			}
		}
		#endregion

		// Use this for initialization
		void Start () {
		
		}
	
		// Update is called once per frame
		void Update () {
			if (Application.isEditor)
			{
				EditorUpdate();
			}
			else
			{
			
			}
		}

		void AddPanel(Vector3 position, float width, float height)
		{
			Vector3 radius = position - transform.position;
	    
			GameObject panelObj = new GameObject();
	    
			// child the panel to this object
			panelObj.transform.parent = transform;
	    
			// initialize mesh
			panelObj.AddComponent<VRUI_Panel>();
			VRUI_Panel panel = panelObj.GetComponent<VRUI_Panel>();
			panel.SetShapeParams(width, height, radius.magnitude);

			// position:
			panelObj.transform.position = position;
	    
			// point towards axis:
			Vector3 axisPoint = new Vector3(transform.position.x, position.y, transform.position.z);
			panelObj.transform.LookAt(axisPoint);
			panelObj.transform.rotation *= Quaternion.Euler(0f,180f,0f); // currently, mesh is backwards :/
	    
	    
		}

	
	}
}

// UI system based on physical movements and interaction