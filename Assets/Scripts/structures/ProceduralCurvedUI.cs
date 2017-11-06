using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
class ProceduralCurvedUI : MonoBehaviour
{
    // we want this number to be high, because one unit is a meter in VR
    public float SquaresPerUnit = 50;

    public float Width;
    public float Height;
    public float Radius;

    private void Start()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = CalculateMesh(Width, Height, Radius);
        // meshrenderer.material = material;
    }


    public Mesh CalculateMesh(float width, float height, float radius)
    {
        Mesh mesh = new Mesh();

        int cols = (int)(width * SquaresPerUnit);
        int rows = (int)(height / SquaresPerUnit);

        

        //derived from arc len formula
        float theta_range_rad = width / (2f * Mathf.PI * radius);
        float theta_offset_rad = (Mathf.PI/2) + (width/2);
        //calculate all the verticies
        //  cols go from left to right, rows from bottom to top
        //  note: for index 'i', row = i floordiv cols+1; col = i mod cols+1
        Vector3[] vertices = new Vector3[(rows + 1) * (cols + 1)];
        for (int col = 0; col < cols+1; col++)
        {
            float arc_position_rad = theta_offset_rad - (col * SquaresPerUnit);
            float x = Mathf.Cos(arc_position_rad);
            float z = Mathf.Sin(arc_position_rad);
            for (int row = 0; row < rows+1; row++)
            {
                float y = (row * SquaresPerUnit) - (.5f * height);
                vertices[row*(cols+1) + col] = new Vector3(x,y,z);
            }
        }
        mesh.vertices = vertices;

        // create the triangles:
        // note: triangles should be in clockwise order
        List<int> triangles = new List<int>();
        for (int col = 0; col < cols; col += 1)
        {
            for (int row = 0; row < rows; row += 1)
            {
                //connect current vertex with those immediately above and to the right
                int bl_i = (row * (cols + 1)) + col;
                int br_i = (row * (cols + 1)) + (col + 1);
                int tl_i = ((row+1) * (cols + 1)) + col;
                int tr_i = ((row+1) * (cols + 1)) + (col + 1);
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

        // do the uv mapping for textures:
        // bottom left: 0,0 ; top right: 1,1
        List<Vector2> uvs = new List<Vector2>();
        float uvx_inc = 1f / (cols + 1);
        float uvy_inc = 1f / (rows + 1);
        for (int row = 0; row < rows+1; row +=1)
        {
            float y = row * uvy_inc;
            for (int col = 0; col < cols+1; col +=1)
            {
                float x = col * uvx_inc;
                uvs.Add(new Vector2(x,y));
            }
        }
        mesh.uv = uvs.ToArray();

        //stuff unity automates:
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }


}
