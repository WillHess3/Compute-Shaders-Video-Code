using UnityEngine;
using System.Collections;

public class SandSimulations : MonoBehaviour {

    public ComputeShader computeShader;
    private RenderTexture displayTexture;
    private RenderTexture previousTexture;

    [Range(1, 100)]
    public int speed;
    private float waitTime;

    public bool play;

    private void Start() {
        displayTexture = new RenderTexture(256, 256, 24);
        displayTexture.enableRandomWrite = true;
        displayTexture.Create();

        previousTexture = new RenderTexture(256, 256, 24);
        previousTexture.enableRandomWrite = true;
        previousTexture.Create();

        computeShader.SetTexture(2, "DisplayTexture", displayTexture);
        computeShader.SetTexture(2, "PreviousTexture", previousTexture);
        computeShader.SetFloat("random", Random.value);
        computeShader.Dispatch(2, displayTexture.width / 8, displayTexture.height / 8, 1);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(displayTexture, destination);
    }

    private IEnumerator NextRound() {
        while (play) {
            computeShader.SetTexture(1, "PreviousTexture", previousTexture);
            computeShader.SetTexture(1, "DisplayTexture", displayTexture);
            computeShader.Dispatch(1, displayTexture.width / 8, displayTexture.height / 8, 1);

            yield return new WaitForSeconds(waitTime);

            computeShader.SetFloat("random", Random.value);
            computeShader.SetTexture(0, "PreviousTexture", previousTexture);
            computeShader.SetTexture(0, "DisplayTexture", displayTexture);
            computeShader.Dispatch(0, displayTexture.width / 8, displayTexture.height / 8, 1);
        }
    }

    private void Update() {
        waitTime = (100 - speed) / 100f;

        if (Input.GetKeyDown(KeyCode.Space)) {
            play = true;
            StartCoroutine(NextRound());
        }
    }
}
