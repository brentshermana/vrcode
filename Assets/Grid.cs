using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

    // we want this number to be high, because one unit is a meter in VR
    public float SquaresPerUnit;

    public float Width;
    public float Height;
    public float Radius;

	private Mesh mesh;
	private Vector3[] vertices;

	private void Awake () {
		Generate();
	}

	private void Generate () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

        int cols = (int)(Width * SquaresPerUnit);
        int rows = (int)(Height * SquaresPerUnit);

        Debug.Log("Rows: " + rows + " Cols: " + cols);

        vertices = new Vector3[(cols + 1) * (rows + 1)];
		Vector2[] uv = new Vector2[vertices.Length];
        float theta_range_rad = Width / (2f * Mathf.PI * Radius);
        float theta_offset_rad = (Mathf.PI / 2) + (Width / 2);
        for (int i = 0, row = 0; row < rows+1; row++) {
			for (int col = 0; col < cols+1; col++, i++) {
                float arc_position_rad = theta_offset_rad - (col/(float)SquaresPerUnit/(Radius));
                float x_point = Mathf.Cos(arc_position_rad) * Radius;
                float z_point = Mathf.Sin(arc_position_rad) * Radius;
                float y_point = ((row/((float)rows))*Height) - (.5f * Height);
                Vector3 position = new Vector3(x_point, y_point, z_point);
                vertices[i] = position;

                float scalef = .01f;
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = position + transform.position;
                go.transform.localScale = new Vector3(scalef, scalef, scalef);

                Vector2 uvvec = new Vector2((float)col / (cols), (float)row / (rows));

                Debug.Log(uvvec);


                uv[i] = new Vector2((float)col / (cols), (float)row / (rows));
			}
		}
		mesh.vertices = vertices;
		mesh.uv = uv;

        List<int> triangles = new List<int>();
        for (int row = 0; row < rows; row += 1)
        {
            for (int col = 0; col < cols; col += 1)
            {
                //connect current vertex with those immediately above and to the right
                int bl_i = (row * (cols + 1)) + col;
                int br_i = (row * (cols + 1)) + (col + 1);
                int tl_i = ((row + 1) * (cols + 1)) + col;
                int tr_i = ((row + 1) * (cols + 1)) + (col + 1);
                //top left triangle:
                triangles.Add(bl_i);
                triangles.Add(tl_i);
                triangles.Add(tr_i);
                //bottom right triangle:
                triangles.Add(bl_i);
                triangles.Add(tr_i);
                triangles.Add(br_i);
            }
        }

        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}