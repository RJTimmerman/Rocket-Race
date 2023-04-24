using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class RedButton : MonoBehaviour
{
    [SerializeField] private Pawn[] players;
    public Pawn currentPlayer;
    private int currentPlayerID = 0;

    [SerializeField] [Min(1)] private int minSteps = 1;
    [SerializeField] [Min(1)] private int maxSteps = 6;
    [SerializeField] private bool doMax = false;  // For testing purposes
    [SerializeField] private bool requireExactEnding = false;  // Then bounce back from the rocket

    private bool active = true;

    private TextMeshProUGUI stepsNumber;


    private void Awake()
    {
        stepsNumber = GameObject.Find("Steps Number").GetComponent<TextMeshProUGUI>();

        if (requireExactEnding)
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

    private static void SetAllPrevTiles()
    {
        Tile tileZero = GameObject.Find("Starting Spot").GetComponent<Tile>();

        void SetPrevTiles(Tile tile)
        {
            foreach (Tile nextTile in tile.nextTiles)
            {
                if (nextTile.prevTiles.Length == 0)
                {
                    nextTile.prevTiles = new Tile[] { tile };
                    SetPrevTiles(nextTile);
                }
                else  // This isn't generally good performance-wise, but as most tiles should only have 1 prevTile, it is probably better than some alternatives
                    nextTile.prevTiles = nextTile.prevTiles.Append(tile).ToArray();
            }
        }
        
        SetPrevTiles(tileZero);
    }
}
