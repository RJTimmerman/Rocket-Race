using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;

public class RedButton : MonoBehaviour
{
    [SerializeField] private Pawn[] players;
    public Pawn currentPlayer;
    private int currentPlayerID = 0;

    [SerializeField] [Min(1)] private int minSteps = 1;
    [SerializeField] [Min(1)] private int maxSteps = 6;

    private bool active = true;
    private bool halted = false;

    private TextMeshProUGUI stepsNumber;


    private void Awake()
    {
        stepsNumber = GameObject.Find("Steps Number").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        Pawn.OnHaltingTile += HaltTurn;
        
        currentPlayer = players[currentPlayerID];
    }

    private void OnMouseDown()
    {
        if (!active) return;
        active = false;

        StartCoroutine( PlayTurn() );
    }

    private IEnumerator PlayTurn()
    {
        int steps = GetSteps();
        for (int step = 0; step < steps; step++)
            if(!halted)
                yield return StartCoroutine( currentPlayer.Move() );

        currentPlayerID++;
        if (currentPlayerID == players.Length) currentPlayerID = 0;
        currentPlayer = players[currentPlayerID];
        active = true; halted = false;
    }
    private void HaltTurn() { halted = true; }

    private int GetSteps()
    {
        int steps = Random.Range(minSteps, maxSteps + 1);
        stepsNumber.text = steps.ToString();
        return steps;
    }

    private void OnDestroy()
    {
        Pawn.OnHaltingTile -= HaltTurn;
    }
}
