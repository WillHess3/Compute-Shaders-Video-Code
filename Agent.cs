using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    private RenderTexture texture;
    public ComputeShader computeShader;

    [Range(0,500)]
    public int speed;
    [Range(0, 1)]
    public float trailBlurAmt;
    [Range(0, 5)]
    public float trailDuration;

    private bool followCursor = true;

    private struct AgentStruct {
        public Vector2 pos;
        public float rot;
        public float rotDelta;
        public Vector3 color;
    }

    private AgentStruct[] data;

    private float timer;

    private void Start() {
        texture = new RenderTexture(1920, 1080, 24);
        texture.enableRandomWrite = true;
        texture.Create();

        computeShader.SetTexture(0, "Result", texture);

        data = new AgentStruct[200000];

        for (int i = 0; i < data.Length; i++) {
            data[i].pos = new Vector2(Random.value * texture.width, Random.value * texture.height);
            data[i].rot = Random.value * 2 * Mathf.PI;
            data[i].rotDelta = Random.Range(-3, 3);
            Color color = Random.ColorHSV();
            data[i].color = new Vector3(color.r, color.g, color.b);
        }

        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float) * 7);
        buffer.SetData(data);

        computeShader.SetBuffer(0, "buffer", buffer);
        computeShader.SetInt("numOfAgents", data.Length);
        computeShader.SetInt("width", texture.width);
        computeShader.SetInt("height", texture.height);
        computeShader.SetInt("speed", speed);
        computeShader.SetFloat("deltaTime", Time.deltaTime);

        float[] mousePos = new float[2];
        mousePos[0] = Input.mousePosition.x;
        mousePos[1] = Input.mousePosition.y;
        computeShader.SetFloats("mousePos", mousePos);

        computeShader.SetBool("followCursor", followCursor);

        computeShader.Dispatch(0, texture.width / 16, 1, 1);
        buffer.GetData(data);
        buffer.Dispose();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(texture, destination);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            followCursor = !followCursor;
        }

        computeShader.SetTexture(0, "Result", texture);

        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float) * 7);
        buffer.SetData(data);
        computeShader.SetBuffer(0, "buffer", buffer);

        computeShader.SetInt("numOfAgents", data.Length);
        computeShader.SetInt("width", texture.width);
        computeShader.SetInt("height", texture.height);
        computeShader.SetInt("speed", speed);
        computeShader.SetFloat("deltaTime", Time.deltaTime);

        float[] mousePos = new float[2];
        mousePos[0] = Input.mousePosition.x;
        mousePos[1] = Input.mousePosition.y;
        computeShader.SetFloats("mousePos", mousePos);

        computeShader.SetBool("followCursor", followCursor);

        computeShader.Dispatch(0, texture.width / 16, 1, 1);
        buffer.GetData(data);

        /*if (timer >= 3) {
            timer = 0;
            for (int i = 0; i < data.Length; i++) {
                data[i].rotDelta = Random.Range(-3, 3);
            }
        } else {
            timer += Time.deltaTime;
        }*/

        buffer.Dispose();

        computeShader.SetTexture(1, "Result", texture);
        computeShader.SetFloat("trailBlurAmt", trailBlurAmt);
        computeShader.SetFloat("trailDuration", trailDuration);
        computeShader.Dispatch(1, texture.width / 8, texture.height / 8, 1);
    }

}
