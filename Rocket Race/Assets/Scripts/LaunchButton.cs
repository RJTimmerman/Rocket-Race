using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchButton : MonoBehaviour
{
    [SerializeField] private Tile.TileColour colour;
    private MeshRenderer colourIndicator;
    private MaterialPropertyBlock mpBlock;

    public static bool doEmptyTileParticles;
    [SerializeField] private bool puffEmptyTiles;


    private void OnValidate()
    {
        doEmptyTileParticles = puffEmptyTiles;
        
        colourIndicator = transform.Find("Colour Indicator").GetComponent<MeshRenderer>();
        mpBlock = new MaterialPropertyBlock();
        
        mpBlock.SetColor("_Color", TileColourColour[colour]);
        colourIndicator.SetPropertyBlock(mpBlock, 0);
    }

    private void Awake()
    {
    }
    
    public static event Action<Tile.TileColour> OnTileEject;
    private void OnMouseDown()
    {
        OnTileEject?.Invoke(colour);
    }

    private void LaunchRocket()
    {
        
    }

    private static Dictionary<Tile.TileColour, Color> TileColourColour = new Dictionary<Tile.TileColour, Color>()
    {
        { Tile.TileColour.Red, Color.red },
        { Tile.TileColour.Blue, Color.blue },
        { Tile.TileColour.Green, Color.green },
        { Tile.TileColour.Yellow, Color.yellow },
        { Tile.TileColour.Orange, new Color(1f, 0.5f, 0f) },
        { Tile.TileColour.Purple, new Color(0.5f, 0f, 0.5f) },
        { Tile.TileColour.Rocket, Color.black }
    };
}
