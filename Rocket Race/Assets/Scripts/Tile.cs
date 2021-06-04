using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool taken = false;
    public enum TileColour { Red, Blue, Green, Yellow, Orange, Purple, Brown, Safe }
    public TileColour colour;
    public bool halt = false;
    public Tile[] nextTiles;
    public bool clickable = false;

    private ParticleSystem highlight;


    private void Awake()
    {
        highlight = GetComponentInChildren<ParticleSystem>();
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
}
