using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchButton : MonoBehaviour
{
    [SerializeField] private Tile.TileColour colour;

    public static bool doEmptyTileParticles;
    [SerializeField] private bool puffEmptyTiles;


    private void OnValidate()
    {
        doEmptyTileParticles = puffEmptyTiles;
    }
    
    public static event Action<Tile.TileColour> OnTileEject;
    private void OnMouseDown()
    {
        OnTileEject?.Invoke(colour);
    }

    private void LaunchRocket()
    {
        
    }
}
