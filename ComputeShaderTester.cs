using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTester : MonoBehaviour {

    public ComputeShader computeShader;

    public RenderTexture renderTexture;

    public int count;
    public int repetitions;

    public Mesh cube;
    public Material material;

    private List<GameObject> objects;

    private Cube[] data;

    private void Start() {
        /*renderTexture = new RenderTexture(256, 256, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);*/
    }

    /*private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        if (renderTexture != null) {
            renderTexture = new RenderTexture(256, 256, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetFloat("Resolution", renderTexture.width);
        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);

        Graphics.Blit(renderTexture, destination);
    }*/

    public void CreateCubes() {
        objects = new List<GameObject>();
        data = new Cube[count * count];

        for (int x = 0; x < count; x++) {
            for (int y = 0; y < count; y++) {
                CreateCube(x, y);
            }
        }
    }

    private void CreateCube(int x, int y) {
        GameObject cube = new GameObject("cube", typeof(MeshFilter), typeof(MeshRenderer));

        cube.GetComponent<MeshFilter>().mesh = this.cube;
        cube.GetComponent<MeshRenderer>().material = new Material(material);
        cube.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());
        cube.transform.position = new Vector3(x - count / 2, y - count / 2, count - Random.Range(-1, 1));

        objects.Add(cube);

        Cube cubeData = new Cube();
        cubeData.position = cube.transform.position;
        cubeData.color = cube.GetComponent<MeshRenderer>().material.color;
        data[x * count + y] = cubeData;
    }

    public void OnRandomizeCPU() {
        for (int i = 0; i < repetitions; i++) {
            for (int j = 0; j < objects.Count; j++) {
                GameObject cube = objects[j];
                cube.GetComponent<MeshRenderer>().material.SetColor("_Color", Random.ColorHSV());
                cube.transform.position = new Vector3(cube.transform.position.x, cube.transform.position.y, count - Random.Range(-1, 1));
            }
        }
    }

    public void OnRandomizeGPU() {
        int colorSize = sizeof(float) * 4;
        int positionSize = sizeof(float) * 3;
        int totalSize = colorSize + positionSize;
        ComputeBuffer computeBuffer = new ComputeBuffer(data.Length, totalSize);
        computeBuffer.SetData(data);

        computeShader.SetBuffer(0, "cubes", computeBuffer);
        computeShader.SetFloat("resolution", data.Length);
        computeShader.SetFloat("count", count);
        computeShader.SetFloat("repetitions", repetitions);
        computeShader.Dispatch(0, data.Length / 10, 1, 1);

        computeBuffer.GetData(data);

        for (int i = 0; i < objects.Count; i++) {
            GameObject obj = objects[i];
            Cube cube = data[i];
            obj.transform.position = cube.position;
            obj.GetComponent<MeshRenderer>().material.color = cube.color;
        }

        computeBuffer.Dispose();
    }

    private void OnGUI() {
        if (objects == null) {
            if (GUI.Button(new Rect(0, 0, 100, 50), "create")) {
                CreateCubes();
            }
        } else {
            if (GUI.Button(new Rect(0, 0, 100, 50), "Random CPU")) {
                OnRandomizeCPU();
            } else if (GUI.Button(new Rect(210, 0, 100, 50), "Random GPU")) {
                OnRandomizeGPU();
            }
        }
    }

}

public struct Cube {
    public Vector3 position;
    public Color color;
}
