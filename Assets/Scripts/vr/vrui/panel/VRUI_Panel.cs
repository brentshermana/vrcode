using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace vrcode.vr.vrui.panel
{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
	[ExecuteInEditMode]
	public class VRUI_Panel : MonoBehaviour
	{
		// for resize event
		protected delegate void resizeDelegate(PanelShape shape); 
		protected event resizeDelegate OnResize;

		// generates the mesh
		public VRUI_PanelGenerator generator;

		public void Resize(PanelShape shape)
		{
			OnResize.Invoke(shape);
			//OnResize(shape);
		}

		protected void GenerateMesh(PanelShape shape)
		{
			generator = new VRUI_PanelGenerator(shape);

			Mesh m = generator.GeneratePanel();

			GetComponent<MeshFilter>().sharedMesh = m;
			GetComponent<MeshCollider>().sharedMesh = m;
		}

	

		// Use this for initialization
		public void Awake ()
		{
			Init();
		}

		// unity doesn't let us override Start itself
		protected virtual void Init()
		{
			OnResize += GenerateMesh;
		}
	
		// Update is called once per frame
		void Update () {
		
		}
	}
}