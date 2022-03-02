using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles : MonoBehaviour {

    public ComputeShader computeShader;
    private RenderTexture texture;

    private struct Particle {
        public Vector2 position;
    }

    private Particle[] data;
    private Particle[] particles;

    [Range(100, 10000)]
    public int dots;

    private void Start() {
        texture = new RenderTexture(256, 256, 24);
        texture.enableRandomWrite = true;
        texture.Create();

        data = new Particle[dots];
        particles = new Particle[dots];
        for (int i = 0; i < data.Length; i++) {
            data[i].position = new Vector2((int)(Random.value * 1920), (int)(Random.value * 1080));
            particles[i] = data[i];
        }

        computeShader.SetTexture(0, "Result", texture);

        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float) * 2);
        buffer.SetData(data);
        computeShader.SetBuffer(0, "buffer", buffer);

        computeShader.SetInt("count", data.Length);

        computeShader.Dispatch(0, texture.width / 8, texture.height / 8, 1);
        buffer.Dispose();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (texture != null) {
            texture = new RenderTexture(1920, 1080, 24);
            texture.enableRandomWrite = true;
            texture.Create();
        }

        computeShader.SetTexture(0, "Result", texture);

        data = new Particle[dots];
        particles = new Particle[dots];
        for (int i = 0; i < data.Length; i++) {
            data[i].position = new Vector2((int)(Random.value * 1920), (int)(Random.value * 1080));
            particles[i] = data[i];
        }

        ComputeBuffer buffer = new ComputeBuffer(data.Length , sizeof(float) * 2);
        buffer.SetData(data);
        computeShader.SetBuffer(0, "buffer", buffer);

        computeShader.SetInt("count", data.Length);

        /*float[] color = new float[3];
        color[0] = Random.value;
        color[1] = Random.value;
        color[2] = Random.value;
        computeShader.SetFloats("randomColor", color);*/

        computeShader.Dispatch(0, texture.width / 8, texture.height / 8, 1);
        buffer.GetData(particles);
        Graphics.Blit(texture, destination);

        buffer.Dispose();
    }

}
