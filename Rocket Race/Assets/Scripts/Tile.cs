using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool taken = false;
    public enum TileColour { Red, Blue, Green, Yellow, Orange, Purple, Brown, Safe, Rocket }
    public TileColour colour;
    public bool halt = false;
    public Tile[] nextTiles;
    public bool clickable = false;

    private ParticleSystem highlight;
    private ParticleSystem puff;


    private void Awake()
    {
        highlight = transform.Find("Selectable Particles").GetComponent<ParticleSystem>();
        if (colour != TileColour.Safe) puff = transform.Find("Ejection Particles").GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        LaunchButton.OnTileEject += EjectPressed;
    }

    public void Highlight()
    {
        clickable = true;
        highlight.Play();
    }

    public void Unhighlight()
    {
        clickable = false;
        highlight.Stop();
    }

    public static event Action<Tile> OnTileSelected; 
    private void OnMouseDown()
    {
        if (!clickable) return;

        OnTileSelected?.Invoke(this);
    }

    private void EjectPressed(TileColour tileColour)
    {
        if (colour == tileColour && (LaunchButton.doEmptyTileParticles || taken)) puff.Play();
    }
}
