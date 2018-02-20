using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VRUI_PanelGenerator
{

	private static float DefaultSquaresPerUnit = 30f;
	private static float DefaultScreenDepth = 0.02f;
	
    
    private float ScreenWidth;
    private float ScreenHeight;
    private float CurveRadius;
	
	private float SquaresPerUnit;
	private float ScreenDepth;

	private int rows;
	private int cols;

	public VRUI_PanelGenerator(float w, float h, float r)
	{
		ScreenWidth = w;
		ScreenHeight = h;
		CurveRadius = r;
		
		// default values
		SquaresPerUnit = DefaultSquaresPerUnit;
		ScreenDepth = DefaultScreenDepth;
		
		// we're going to want to ensure that the back and front screen have the same
		// number of rows and columns
		rows = Mathf.CeilToInt(ScreenWidth * SquaresPerUnit);
		cols = Mathf.CeilToInt(ScreenHeight * SquaresPerUnit);
	}

	public Mesh GeneratePanel()
	{
		Mesh screen = GenerateScreen();
		Mesh backboard = GenerateBackboard(screen);
		return backboard;
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
        Vector3 shift = new Vector3(0f,0f,ScreenDepth);
        for (int i = 0; i < originalVerticesLen; i += 1)
        {
            Vector3 newVert = vertices[i] + shift;
            vertices.Add(newVert);
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

        // we want the backboard to have the same uv coordinates along the front panel
        List<Vector2> uv = new List<Vector2>(screen.uv);
        Vector2 dummy = new Vector2(0f,0f);
        while (uv.Count < vertices.Count)
        {
            uv.Add(dummy);
        }
        backboard.uv = uv.ToArray();

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
       

        Vector3[] vertices = new Vector3[(cols + 1) * (rows + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        //float theta_range_rad = ScreenWidth / (2f * Mathf.PI * CurveRadius);
	    float theta_range_rad = Mathf.Abs(ScreenWidth / CurveRadius);
        for (int i = 0, row = 0; row < rows + 1; row++)
        {
	        float axisPosition = ((row / ((float) rows)) * ScreenHeight) - (.5f * ScreenHeight);
	        var arcIterator = new ArcIterator(Vector3.up, Vector3.up * axisPosition, CurveRadius)
		        .Iterator(-theta_range_rad/2f, theta_range_rad/(cols+1))
		        .GetEnumerator();
            for (int col = 0; col < cols + 1; col++, i++)
            {
	            arcIterator.MoveNext();
                Vector3 position = arcIterator.Current;
	            
                vertices[i] = position;

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
