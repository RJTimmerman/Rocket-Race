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
    [SerializeField] private bool doMax = false;  // For testing purposes
    [SerializeField] private bool requireExactEnding = false;  // Otherwise bounce back from the rocket

    private bool active = true;

    private TextMeshProUGUI stepsNumber;


    private void Awake()
    {
        stepsNumber = GameObject.Find("Steps Number").GetComponent<TextMeshProUGUI>();

        SetAllPrevTiles();
    }

    private void Start()
    {
        (currentPlayer = players[currentPlayerID]).inPlay = true;
    }

    private void OnMouseDown()
    {
        if (!active)
            return;
        active = false;

        StartCoroutine( PlayTurn() );
    }

    private IEnumerator PlayTurn()
    {
        yield return StartCoroutine( currentPlayer.Move(GetSteps(), requireExactEnding) );

        currentPlayer.inPlay = false;
        if (++currentPlayerID == players.Length)
            currentPlayerID = 0;
        (currentPlayer = players[currentPlayerID]).inPlay = true;
        active = true;
    }

    private int GetSteps()
    {
        int steps = doMax ? maxSteps : Random.Range(minSteps, maxSteps + 1);
        stepsNumber.text = steps.ToString();
        return steps;
    }

    private void SetAllPrevTiles()
    {
        throw new NotImplementedException("Trying to set all previous tile arrays.");
    }
}
