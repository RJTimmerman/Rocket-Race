using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchButton : MonoBehaviour
{
    public Tile.TileColour colour;
    public bool rocket;
    

    public static event Action<Tile.TileColour> OnTileEject;
    private void OnMouseDown()
    {
        if (rocket) LaunchRocket();
        else OnTileEject?.Invoke(colour);
    }

    private void LaunchRocket()
    {
        
    }
}
