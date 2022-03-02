using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fractal : MonoBehaviour {

    private float width, height;
    private float rStart, iStart;
    private int increments, maxIterations;
    private float zoom;

    public ComputeShader computeShader;
    private ComputeBuffer computeBuffer;
    public RawImage image;
    private RenderTexture texture;

    private struct FractalDataStruct {
        public float width, height, rStart, iStart;
        public int screenWidth, screenHeight;
    }

    private FractalDataStruct[] data;

    private void Start() {
        width = 5;
        height = 2.5f;
        rStart = -3.25f;
        iStart = -4.1f;
        maxIterations = 1000;
        increments = 3;
        zoom = 0.5f;

        data = new FractalDataStruct[1];
        data[0].width = width;
        data[0].height = height;
        data[0].rStart = rStart;
        data[0].iStart = iStart;
        data[0].screenWidth = Screen.width;
        data[0].screenHeight = Screen.height;

        computeBuffer = new ComputeBuffer(data.Length, sizeof(int) * 2 + sizeof(float) * 4);

        texture = new RenderTexture(Screen.width, Screen.height, 0);
        texture.enableRandomWrite = true;
        texture.Create();

        MakeFractal();
    }

    private void MakeFractal() {
        computeBuffer.SetData(data);

        computeShader.SetBuffer(0, "buffer", computeBuffer);
        computeShader.SetTexture(0, "Result", texture);
        computeShader.SetInt("maxItterations", maxIterations);

        computeShader.Dispatch(0, Screen.width / 8, Screen.height / 8, 1);

        RenderTexture.active = texture;
        image.material.mainTexture = texture;
    }

    private void OnDestroy() {
        computeBuffer.Dispose();
    }

    private void RecenterFractal() {
        rStart += (Input.mousePosition.x - (Screen.width / 2f)) / Screen.width * width;
        iStart += (Input.mousePosition.y - (Screen.height / 2f)) / Screen.height * height;

        data[0].rStart = rStart;
        data[0].iStart = iStart;

        MakeFractal();
    }

    private void ZoomIn() {
        maxIterations = Mathf.Max(100, maxIterations + increments);

        float widthFactor = width * zoom * Time.deltaTime;
        float heightFactor = height * zoom * Time.deltaTime;

        width -= widthFactor;
        height -= heightFactor;

        rStart += widthFactor / 2f;
        iStart += heightFactor / 2f;

        data[0].rStart = rStart;
        data[0].iStart = iStart;
        data[0].width = width;
        data[0].height = height;

        MakeFractal();
    }

    private void ZoomOut() {
        maxIterations = Mathf.Max(100, maxIterations - increments);

        float widthFactor = width * zoom * Time.deltaTime;
        float heightFactor = height * zoom * Time.deltaTime;

        width += widthFactor;
        height += heightFactor;

        rStart -= widthFactor / 2f;
        iStart -= heightFactor / 2f;

        data[0].rStart = rStart;
        data[0].iStart = iStart;
        data[0].width = width;
        data[0].height = height;

        MakeFractal();
    }

    private void Update() {
        if (Input.GetMouseButton(0)) {
            ZoomIn();
        } else if (Input.GetMouseButton(1)) {
            ZoomOut();
        }

        if (Input.GetMouseButtonDown(2)) {
            RecenterFractal();
        }
    }
}
