using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreedyPiggy : MonoBehaviour {

    public ComputeShader computeShader;
    public int sitAfterRolls;
    public int sitAfterPoints;
    public bool rolls;
    public int simulatedRounds;

    private struct Round {
        public int points;
        public int points16;
        public float random;
    }

    private Round[] rounds;

    private int total;
    public int numOfGames;

    public bool run;

    private Vector2 wins;
    private int games;

    private void Start() {
        rounds = new Round[simulatedRounds];

        /*for (int i = 0; i < rounds.Length; i++) {
            rounds[i].random = Random.value;
        }

        ComputeBuffer buffer = new ComputeBuffer(rounds.Length, sizeof(int) * 2 + sizeof(float));
        buffer.SetData(rounds);

        computeShader.SetBool("sitAfterRolls", rolls);
        computeShader.SetBuffer(0, "rounds", buffer);
        computeShader.SetInt("sitAfterR", sitAfterRolls);
        computeShader.SetInt("sitAfterP", sitAfterPoints);
        computeShader.Dispatch(0, simulatedRounds / 2, 1, 1);

        buffer.GetData(rounds);
        buffer.Dispose();

        int sum = 0;
        int sum16 = 0;
        int zeroRounds = 0;
        for (int i = 0; i < simulatedRounds; i++) {
            sum += rounds[i].points;
            sum16 += rounds[i].points16;
            if (rounds[i].points == 0) {
                zeroRounds++;
            }
        }

        Debug.Log("16: " + sum16 + ", 5 15: " + sum);
        //Debug.Log(sum + "; " + zeroRounds / (float)simulatedRounds);
        //StartCoroutine(AverageSimulatedRounds());*/
    }

    private IEnumerator AverageSimulatedRounds() {
        for (int h = 0; h < numOfGames; h++) {
            for (int i = 0; i < rounds.Length; i++) {
                rounds[i].random = Random.value;
            }

            ComputeBuffer buffer = new ComputeBuffer(rounds.Length, sizeof(int) + sizeof(float));
            buffer.SetData(rounds);

            computeShader.SetBool("sitAfterRolls", rolls);
            computeShader.SetInt("sitAfterR", sitAfterRolls);
            computeShader.SetInt("sitAfterP", sitAfterPoints);
            computeShader.SetBuffer(0, "rounds", buffer);
            computeShader.Dispatch(0, simulatedRounds / 8, 1, 1);

            buffer.GetData(rounds);
            buffer.Dispose();

            for (int i = 0; i < simulatedRounds; i++) {
                total += rounds[i].points;
            }

            yield return new WaitForSeconds(5);
            Debug.Log(total);
        }

        Debug.Log(total / (float)numOfGames);
    }

    private void Update() {
        while (games < 10000) {
            games++;

            for (int i = 0; i < rounds.Length; i++) {
                rounds[i].random = Random.value * 100;
                rounds[i].points = 0;
                rounds[i].points16 = 0;
            }

            ComputeBuffer buffer = new ComputeBuffer(rounds.Length, sizeof(int) * 2 + sizeof(float));
            buffer.SetData(rounds);

            computeShader.SetBuffer(0, "rounds", buffer);
            computeShader.Dispatch(0, simulatedRounds / 2, 1, 1);

            buffer.GetData(rounds);
            buffer.Dispose();

            int sum = 0;
            int sum16 = 0;
            for (int i = 0; i < simulatedRounds; i++) {
                sum += rounds[i].points;
                sum16 += rounds[i].points16;
            }

            if (sum16 > sum) {
                wins.x++;
            } else { 
                wins.y++; 
            }

            Debug.Log(wins);
        }
    }
}
