using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool taken = false;
    public enum TileColour { Red, Blue, Green, Yellow, Orange, Purple, Safe, Rocket, Start }
    public TileColour colour;
    public bool halt = false;
    public Tile[] nextTiles;
    [HideInInspector] public Tile[] prevTiles = Array.Empty<Tile>();  // For bouncing back, created based on nextTiles (if required)  // I believe Unity already makes this an empty array, so I made it explicit and assume non-nullity in SetPrevTiles
    public bool clickable = false;
    private List<Tile> currentPath;

    [CanBeNull] private ParticleSystem highlight;
    [CanBeNull] private ParticleSystem puff;


    private void Awake()
    {
        if (colour != TileColour.Start && colour != TileColour.Rocket)
            highlight = transform.Find("Selectable Particles").GetComponent<ParticleSystem>();
        if (colour != TileColour.Start && colour != TileColour.Rocket && colour != TileColour.Safe)
            puff = transform.Find("Ejection Particles").GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        LaunchButton.OnTileEject += EjectPressed;
    }

    public void Highlight(List<Tile> path)
    {
        clickable = true;
        currentPath = path;
        highlight?.Play();
    }

    public void Unhighlight()
    {
        clickable = false;
        highlight?.Stop();
    }

    public static event Action<List<Tile>> OnTileSelected; 
    private void OnMouseDown()
    {
        if (!clickable)
            return;

        OnTileSelected?.Invoke(currentPath);
    }

    private void EjectPressed(TileColour tileColour, bool puffEmptyTiles)
    {
        if (colour == tileColour && (puffEmptyTiles || taken))
            puff?.Play();
    }
}
