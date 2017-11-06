using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

    public bool DebugMesh;

    // we want this number to be high, because one unit is a meter in VR
    public float SquaresPerUnit;

    public float ScreenWidth;
    public float ScreenHeight;
    public float CurveRadius;

    public float BackboardThickness;
    public Material BackboardMaterial;

	private Mesh mesh;
	private Vector3[] vertices;

    private int rows;
    private int cols;

	private void Awake () {
		Generate();
	}

	private void Generate () {
        Mesh screen = GenerateScreen();
        GetComponent<MeshFilter>().mesh = GenerateScreen();

        //create the child which will contain the backboard
        GameObject childO = new GameObject();
        
        childO.transform.position = transform.position;
        childO.transform.rotation = transform.rotation;
        childO.transform.parent = transform;

        childO.AddComponent<MeshFilter>();
        childO.AddComponent<MeshRenderer>();

        Mesh Backboard = GenerateBackboard(screen);

        childO.GetComponent<MeshFilter>().mesh = Backboard;
        childO.GetComponent<MeshRenderer>().material = BackboardMaterial;
    }

    private void debugMeshPoint(Vector3 point)
    {
        if (DebugMesh) {
            float scalef = .01f;
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            go.transform.parent = transform;
            go.transform.localPosition = point;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = new Vector3(scalef, scalef, scalef);
        }
    }

    private Mesh GenerateBackboard(Mesh screen)
    {
        Mesh backboard = new Mesh();

        // copy over current values from screen mesh
        List<Vector3> vertices = new List<Vector3>(screen.vertices);
        List<int> triangles = new List<int>(screen.triangles);

        // create a new set of vertices by shifting current ones back
        int originalVerticesLen = vertices.Count;
        //TODO: if we want this to generalize to arc angles >= 90*,
        //  we need to do more trig calculations for 'shift'
        Vector3 shift = new Vector3(0f,0f,BackboardThickness);
        for (int i = 0; i < originalVerticesLen; i += 1)
        {
            Vector3 newVert = vertices[i] + shift;
            vertices.Add(newVert);
            debugMeshPoint(newVert);
        }
        backboard.vertices = vertices.ToArray();

        //now add more triangles for back face
        int originalTrianglesLength = triangles.Count;
        for (int i = 0; i < originalTrianglesLength; i+=3)
        {
            //because the back of the backboard is facing the opposite direction,
            // we need to switch clockwise to counterclockwise:
            triangles.Add(triangles[i] + originalVerticesLen);
            triangles.Add(triangles[i+2] + originalVerticesLen);
            triangles.Add(triangles[i+1] + originalVerticesLen);
        }

        // finally, 'knit' the sides together:
        for (int row = 0; row < rows; row += 1)
        {
            // 'left' side: all rows, col: 0
            int col = 0;

            int front_bottom_i = (row * (cols + 1)) + col;
            int front_top_i = ((row + 1) * (cols + 1)) + col;
            int back_bottom_i = front_bottom_i + originalVerticesLen;
            int back_top_i = front_top_i + originalVerticesLen;

            triangles.AddRange(new int[]{ back_bottom_i, front_top_i, front_bottom_i });
            triangles.AddRange(new int[] { back_bottom_i, back_top_i, front_top_i });

            // 'right' side: all rows, col = cols
            col = cols;

            front_bottom_i = (row * (cols + 1)) + col;
            front_top_i = ((row + 1) * (cols + 1)) + col;
            back_bottom_i = front_bottom_i + originalVerticesLen;
            back_top_i = front_top_i + originalVerticesLen;

            triangles.AddRange(new int[] { back_bottom_i, front_bottom_i, front_top_i });
            triangles.AddRange(new int[] { back_bottom_i, front_top_i, back_top_i });
        }
        // ... and the bottom and top:
        for (int col = 0; col < cols; col += 1)
        {
            // top: row = rows
            int row = rows;

            int front_left_i = (row * (cols + 1)) + col;
            int front_right_i = (row * (cols + 1)) + (col + 1);
            int back_left_i = front_left_i + originalVerticesLen;
            int back_right_i = front_right_i + originalVerticesLen;

            triangles.AddRange(new int[] { back_left_i, front_right_i, front_left_i });
            triangles.AddRange(new int[] { back_left_i, back_right_i, front_right_i });

            // 'bottom' side: all rows, col = cols
            row = 0;

            front_left_i = (row * (cols + 1)) + col;
            front_right_i = ((row) * (cols + 1)) + (col + 1);
            back_left_i = front_left_i + originalVerticesLen;
            back_right_i = front_right_i + originalVerticesLen;

            triangles.AddRange(new int[] { back_left_i, front_left_i, front_right_i });
            triangles.AddRange(new int[] { back_left_i, front_right_i, back_right_i });
        }
        backboard.triangles = triangles.ToArray();

        backboard.RecalculateNormals();
        backboard.RecalculateBounds();
        backboard.RecalculateTangents();

        return backboard;
    }

    private Mesh GenerateScreen()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Curved Screen";

        cols = (int)(ScreenWidth * SquaresPerUnit);
        rows = (int)(ScreenHeight * SquaresPerUnit);
       

        vertices = new Vector3[(cols + 1) * (rows + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        float theta_range_rad = ScreenWidth / (2f * Mathf.PI * CurveRadius);
        float theta_offset_rad = (Mathf.PI / 2) + (ScreenWidth/CurveRadius / 2);
        for (int i = 0, row = 0; row < rows + 1; row++)
        {
            for (int col = 0; col < cols + 1; col++, i++)
            {
                float arc_position_rad = theta_offset_rad - (col / (float)SquaresPerUnit / (CurveRadius));
                float x_point = Mathf.Cos(arc_position_rad) * CurveRadius;
                float z_point = Mathf.Sin(arc_position_rad) * CurveRadius;
                float y_point = ((row / ((float)rows)) * ScreenHeight) - (.5f * ScreenHeight);
                Vector3 position = new Vector3(x_point, y_point, z_point);
                vertices[i] = position;

                debugMeshPoint(position);

                Vector2 uvvec = new Vector2((float)col / (cols), (float)row / (rows));
                
                uv[i] = uvvec;
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
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        return mesh;
    }
}