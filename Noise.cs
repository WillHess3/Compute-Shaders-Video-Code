using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise : MonoBehaviour {

    public RenderTexture texture;

    public ComputeShader computeShader;
    public int width;
    public int height;

    public enum NoiseTypes {
        Simple,
        Vorinoi,
    }

    public NoiseTypes noiseType;

    private struct VorinoiPoint {
        public Vector2 pos;
        public float angle;
        public float speed;
    }

    private VorinoiPoint[] vorinoiPoints;
    public int vorinoiDensity;

    public float vorinoiScale;
    public float vorinoiSpeedMultiplier;

    public Color color1;
    public Color color2;

    private void Start() {
        texture = new RenderTexture(width, height, 24);
        texture.enableRandomWrite = true;
        texture.Create();

        float[] color1Components = new float[4];
        color1Components[0] = color1.r;
        color1Components[1] = color1.g;
        color1Components[2] = color1.b;
        color1Components[3] = 1;


        float[] color2Components = new float[4];
        color2Components[0] = color2.r;
        color2Components[1] = color2.g;
        color2Components[2] = color2.b;
        color2Components[3] = 1;


        computeShader.SetFloats("color1", color1Components);
        computeShader.SetFloats("color2", color2Components);

        if (noiseType == NoiseTypes.Simple) {
            computeShader.SetTexture(0, "Result", texture);
            computeShader.SetInt("width", texture.width);
            computeShader.SetInt("height", texture.height);

            computeShader.Dispatch(0, texture.width / 8, texture.height / 8, 1);
        } else if (noiseType == NoiseTypes.Vorinoi) {

            vorinoiPoints = new VorinoiPoint[vorinoiDensity];

            for (int i = 0; i < vorinoiPoints.Length; i++) {
                vorinoiPoints[i].pos = new Vector2(Random.value * width, Random.value * height);
                vorinoiPoints[i].angle = Random.value * 6.28f;
                vorinoiPoints[i].speed = Random.value / 2 - .5f;
            }

            ComputeBuffer buffer = new ComputeBuffer(vorinoiDensity, sizeof(float) * 4);
            buffer.SetData(vorinoiPoints);

            computeShader.SetBuffer(1, "vorinoiPoints", buffer);
            computeShader.SetTexture(1, "Result", texture);
            computeShader.SetInt("width", texture.width);
            computeShader.SetInt("height", texture.height);
            computeShader.SetFloat("vorinoiScale", vorinoiScale);


            computeShader.Dispatch(1, texture.width / 8, texture.height / 8, 1);

            buffer.Dispose();
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(texture, destination);
    }

    private void Update() {
        if (noiseType == NoiseTypes.Simple) {
            computeShader.SetTexture(0, "Result", texture);
            computeShader.SetInt("width", texture.width);
            computeShader.SetInt("height", texture.height);

            computeShader.Dispatch(0, texture.width / 8, texture.height / 8, 1);
        } else if (noiseType == NoiseTypes.Vorinoi) {
            ComputeBuffer points = new ComputeBuffer(vorinoiDensity, sizeof(float) * 4);
            points.SetData(vorinoiPoints);

            computeShader.SetBuffer(2, "vorinoiPoints", points);
            computeShader.SetFloat("vorinoiSpeedMultiplier", vorinoiSpeedMultiplier * Time.deltaTime);
            computeShader.SetFloat("random", Random.value);

            computeShader.Dispatch(2, vorinoiPoints.Length / 8, texture.height / 8, 1);
            points.GetData(vorinoiPoints);
            points.Dispose();

            ComputeBuffer buffer = new ComputeBuffer(vorinoiDensity, sizeof(float) * 4);
            buffer.SetData(vorinoiPoints);

            computeShader.SetBuffer(1, "vorinoiPoints", buffer);
            computeShader.SetTexture(1, "Result", texture);
            computeShader.SetFloat("vorinoiScale", vorinoiScale);

            computeShader.Dispatch(1, texture.width / 8, texture.height / 8, 1);

            buffer.Dispose();
        }
    }

}
