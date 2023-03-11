using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CreatePointcloud : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Create buffer with 1000 points of x,y,z,r,g,b data
        var data = new NativeArray<PointData>(4, Allocator.Persistent);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = new PointData(i, i, i, 255, 255, 255);
        }

        // Create a new GameObject to hold the point cloud
        var pointCloudObject = new GameObject("PointCloud");

        // Add a MeshFilter component to the GameObject
        var meshFilter = pointCloudObject.AddComponent<MeshFilter>();

        // Create a new Mesh to hold the vertices of the point cloud
        var mesh = new Mesh();

        // Loop through the data buffer and add each point as a vertex to the Mesh
        for (int i = 0; i < data.Length; i++)
        {
            var point = data[i];

            // Create a new vertex and add it to the Mesh
            var vertex = new Vector3(point.x, point.y, point.z);
            mesh.vertices[i] = vertex;

            // Set the color of the vertex based on the r, g, and b values of the corresponding point
            var color = new Color(point.r / 255f, point.g / 255f, point.b / 255f);
            mesh.colors[i] = color;
        }

        // Assign the Mesh to the MeshFilter
        meshFilter.mesh = mesh;

        // Add a MeshRenderer component to the GameObject
        //var meshRenderer = pointCloudObject.AddComponent<MeshRenderer>();

        // Create a new Material and set its shader to a point cloud shader
        //var material = new Material(Shader.Find("Custom/PointCloudShader"));

        // Assign the Material to the MeshRenderer
        //meshRenderer.material = material;
    }

    // Update is called once per frame
    void Update()
    {

    }
}

internal struct PointData
{
    public float x;
    public float y;
    public float z;
    public float r;
    public float g;
    public float b;
    public PointData(float x, float y, float z, float r, float g, float b)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.r = r;
        this.g = g;
        this.b = b;
    }
}