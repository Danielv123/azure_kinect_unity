using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class PointCloudRenderer : MonoBehaviour
{
    Texture2D texColor;
    Texture2D texPosScale;
    VisualEffect vfx;
    uint resolution = 1024;

    public float particleSize = 0.1f;
    bool toUpdate = false;
    uint particleCount = 0;

    AzureKinect device = null;

    // Start is called before the first frame update
    void Start()
    {
        vfx = GetComponent<VisualEffect>();
        device = new AzureKinect();
        device.StartCamera();

        Color[] positions = new Color[resolution * resolution];
        Color[] colors = new Color[resolution * resolution];

        //for(int x = 0; x < resolution; x++)
        //{
        //    for(int y = 0; y < resolution; y++)
        //    {
        //        positions[x + y * (int)resolution] = new Vector3(Random.value * 10, Random.value * 10, Random.value * 10);
        //        colors[x + y * (int)resolution] = new Color(Random.value, Random.value, Random.value);
        //    }
        //}

        SetParticles(positions, colors);
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy - stopping cameras");
        device.StopCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (device != null && device.HasChanged)
        {
            SetParticles(device.positions, device.colors);
            device.HasChanged = false;
        }
        if (toUpdate)
        {
            toUpdate = false;
            vfx.Reinit();
            vfx.SetUInt(Shader.PropertyToID("ParticleCount"), particleCount);
            vfx.SetTexture(Shader.PropertyToID("TexColor"), texColor);
            vfx.SetTexture(Shader.PropertyToID("TexPosScale"), texPosScale);
            vfx.SetUInt(Shader.PropertyToID("Resolution"), resolution);
        }
    }

    public void SetParticles(Color[] positions, Color[] colors)
    {
        resolution = (uint)Math.Sqrt(positions.Length);
        int texWidth = Mathf.Min(positions.Length, (int)resolution);
        int texHeight = Mathf.Max(1, positions.Length / (int)resolution);

        // Only create the texture once and update it. If we keep recreating it we get a memory leak.
        if (!texColor)
        {
            texColor = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false);
        }
        if (!texPosScale)
        {
            texPosScale = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false);
        }

        // Set pixel colors and data using SetPixels method
        //Color[] texColorPixels = new Color[texWidth * texHeight];
        //Color[] texPosScalePixels = new Color[texWidth * texHeight];
        //for (int i = 0; i < positions.Length; i++)
        //{
        //    //texColorPixels[i] = colors[i];
        //    texPosScalePixels[i] = new Color(positions[i].x, positions[i].y, positions[i].z, particleSize);
        //}
        texColor.SetPixels(colors);
        texPosScale.SetPixels(positions);

        texColor.Apply();
        texPosScale.Apply();

        particleCount = (uint)positions.Length;
        toUpdate = true;
    }
}
