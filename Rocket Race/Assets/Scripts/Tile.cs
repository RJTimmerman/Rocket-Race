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
    public bool clickable = false;

    [CanBeNull] private ParticleSystem highlight;
    [CanBeNull] private ParticleSystem puff;


    private void Awake()
    {
        if (colour != TileColour.Start)
            highlight = transform.Find("Selectable Particles").GetComponent<ParticleSystem>();
        if (colour != TileColour.Safe && colour != TileColour.Start)
            puff = transform.Find("Ejection Particles").GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        LaunchButton.OnTileEject += EjectPressed;
    }

    public void Highlight()
    {
        clickable = true;
        highlight?.Play();
    }

    public void Unhighlight()
    {
        clickable = false;
        highlight?.Stop();
    }

    public static event Action<Tile> OnTileSelected; 
    private void OnMouseDown()
    {
        if (!clickable) return;

        OnTileSelected?.Invoke(this);
    }

    private void EjectPressed(TileColour tileColour)
    {
        if (colour == tileColour && (LaunchButton.doEmptyTileParticles || taken))
            puff?.Play();
    }
}
