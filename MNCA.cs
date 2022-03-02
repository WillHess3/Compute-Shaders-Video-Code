using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MNCA : MonoBehaviour {

    public ComputeShader computeShader;
    private RenderTexture displayTexture;
    private RenderTexture previousTexture;

    private bool playing;

    [Range(1, 100)]
    public int speed;
    private float waitTime;

    [Range(0, 1)]
    public float percentAlive;
    private float previousPercentAlive;

    public Color deadColor;
    public Color aliveColor;

    private void Start() {
        displayTexture = new RenderTexture(1920, 1080, 24);
        displayTexture.enableRandomWrite = true;
        displayTexture.Create();

        previousTexture = new RenderTexture(1920, 1080, 24);
        previousTexture.enableRandomWrite = true;
        previousTexture.Create();

        float[] aliveColorComponents = new float[4];
        aliveColorComponents[0] = aliveColor.r;
        aliveColorComponents[1] = aliveColor.g;
        aliveColorComponents[2] = aliveColor.b;
        aliveColorComponents[3] = 1;

        float[] deadColorComponents = new float[4];
        deadColorComponents[0] = deadColor.r;
        deadColorComponents[1] = deadColor.g;
        deadColorComponents[2] = deadColor.b;
        deadColorComponents[3] = 1;

        computeShader.SetFloats("aliveColor", aliveColorComponents);
        computeShader.SetFloats("deadColor", deadColorComponents);
        computeShader.SetFloat("randomX", Random.value);
        computeShader.SetFloat("randomY", Random.value);
        computeShader.SetInt("width", displayTexture.width);
        computeShader.SetInt("height", displayTexture.height);
        computeShader.SetFloat("percentAlive", percentAlive);

        computeShader.SetTexture(1, "PreviousTexture", previousTexture);
        computeShader.SetTexture(1, "DisplayTexture", displayTexture);
        computeShader.Dispatch(1, displayTexture.width / 8, displayTexture.height / 8, 1);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(displayTexture, destination);
    }

    private IEnumerator Play() {
        while (playing) {
            computeShader.SetTexture(2, "DisplayTexture", displayTexture);
            computeShader.SetTexture(2, "PreviousTexture", previousTexture);
            computeShader.Dispatch(2, displayTexture.width / 8, displayTexture.height / 8, 1);

            yield return new WaitForSeconds(waitTime);

            computeShader.SetTexture(0, "DisplayTexture", displayTexture);
            computeShader.SetTexture(0, "PreviousTexture", previousTexture);
            computeShader.SetInt("mouseX", (int)Input.mousePosition.x);
            computeShader.SetInt("mouseY", (int)Input.mousePosition.y);
            computeShader.SetInt("radius", 5000);
            computeShader.Dispatch(0, displayTexture.width / 8, displayTexture.height / 8, 1);

            yield return null;
        }
    }

    private void Update() {
        waitTime = (100 - speed) / 100f;

        if (!playing && previousPercentAlive != percentAlive) {
            computeShader.SetTexture(1, "DisplayTexture", displayTexture);
            computeShader.SetTexture(1, "PreviousTexture", previousTexture);
            computeShader.SetFloat("randomX", Random.value);
            computeShader.SetFloat("randomY", Random.value);
            computeShader.SetFloat("percentAlive", percentAlive);
            computeShader.Dispatch(1, displayTexture.width / 8, displayTexture.height / 8, 1);
        }

        previousPercentAlive = percentAlive;
    }

    private void OnGUI() {
        if (!playing) {
            if (GUI.Button(new Rect(0, 0, Screen.width / 10, Screen.height / 10), "Start")) {
                playing = true;
                StartCoroutine(Play());
            }
        } else {
            if (GUI.Button(new Rect(0, 0, Screen.width / 10, Screen.height / 10), "Stop")) {
                playing = false;
            }
        }
    }

}

