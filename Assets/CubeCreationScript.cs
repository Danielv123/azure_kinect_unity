using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CubeCreationScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //GameObject cube1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        //cube2.transform.position = new Vector3(5, 0, 0);


        // Create buffer with 1000 points of x,y,z,r,g,b data
        var data = new NativeArray<PointData>(4000, Allocator.Temp);

        Console.WriteLine(data.Length);
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = new PointData(
                UnityEngine.Random.Range(-5, 5),
                UnityEngine.Random.Range(-5, 5),
                UnityEngine.Random.Range(-5, 5),
                UnityEngine.Random.Range(0, 1),
                UnityEngine.Random.Range(0, 1),
                UnityEngine.Random.Range(0, 1)
            );
        }

        // Create a new GameObject to hold the point cloud
        var pointCloudObject = new GameObject("PointCloud");

        // Add a MeshFilter component to the GameObject
        var meshFilter = pointCloudObject.AddComponent<MeshFilter>();

        // Create a new Mesh to hold the vertices of the point cloud
        var mesh = new Mesh();

        var vertices = new List<Vector3>(mesh.vertices);
        var colors = new List<Color>(mesh.colors);

        // Loop through the data buffer and add each point as a vertex to the Mesh
        for (int i = 0; i < data.Length; i++)
        {
            var point = data[i];

            // Create a new vertex and add it to the Mesh
            var vertex = new Vector3(point.x, point.y, point.z);
            vertices.Add(vertex);

            // Set the color of the vertex based on the r, g, and b values of the corresponding point
            var color = new Color(point.r, point.g, point.b);
            colors.Add(color);
        }
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Assign the Mesh to the MeshFilter
        meshFilter.mesh = mesh;

        // Add a MeshRenderer component to the GameObject
        var meshRenderer = pointCloudObject.AddComponent<MeshRenderer>();
        var material = new Material(Shader.Find("Standard"));
        meshRenderer.material = material;


    }

    // Update is called once per frame
    void Update()
    {

    }
}
