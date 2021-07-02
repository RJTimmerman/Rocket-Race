using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField] private Color colour;

    private Tile currentTile;
    private Tile nextTile;
    private Tile lastTile;  // This is being stored to make sure you cannot get back onto a safe spot you just came from
    private SkinnedMeshRenderer pawnRenderer;
    private MaterialPropertyBlock mpBlock;
    private Rigidbody rb;
    private Tile tileZero;

    private Vector3 startPos;
    private Quaternion startRot;

    
    private void OnValidate()
    {
        pawnRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        mpBlock = new MaterialPropertyBlock();
        
        mpBlock.SetColor("_Color", colour);
        pawnRenderer.SetPropertyBlock(mpBlock, 1);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        tileZero = GameObject.Find("Starting Spot").GetComponent<Tile>();
        currentTile = tileZero;
        lastTile = tileZero;

        startPos = transform.position;
        startRot = transform.rotation;
    }

    private void Start()
    {
        Tile.OnTileSelected += RegisterTileChoice;
        LaunchButton.OnTileEject += EjectPressed;
    }

    public static event Action OnHaltingTile;
    public IEnumerator Move()
    {
        if (currentTile.nextTiles.Length == 1) nextTile = currentTile.nextTiles[0];
        else
        {
            foreach (Tile tile in currentTile.nextTiles) if (!tile.taken && tile != lastTile) tile.Highlight();

            yield return new WaitWhile(() => nextTile is null);
            
            foreach (Tile tile in currentTile.nextTiles) tile.Unhighlight();
        }
        
        yield return StartCoroutine( MoveTo(transform.position + new Vector3(0, 4, 0), 0.5f) );
        yield return StartCoroutine( MoveTo(nextTile.transform.position + new Vector3(0, 4.15f, 0), 1) );
        yield return StartCoroutine( MoveTo(nextTile.transform.position + new Vector3(0, 0.15f, 0), 1) );

        lastTile = currentTile;
        currentTile = nextTile;
        nextTile = null;
        lastTile.taken = false;
        currentTile.taken = true;
        
        if (currentTile.halt) OnHaltingTile?.Invoke();
    }

    private IEnumerator MoveTo(Vector3 targetPos, float duration)
    {
        float timePassed = 0;
        Vector3 begin = transform.position;
        Vector3 direction = targetPos - begin;
        while(timePassed < duration)
        {
            timePassed += Time.deltaTime;
            transform.Translate(direction / duration * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }

    private void RegisterTileChoice(Tile tile)
    {
        nextTile = tile;
    }

    private void EjectPressed(Tile.TileColour tileColour)
    {
        if (currentTile.colour != tileColour) return;
        StartCoroutine( GetEjected() );
    }

    private IEnumerator GetEjected()
    {
        rb.isKinematic = false;
        rb.AddForce(new Vector3(200, 700, -200));
        rb.AddTorque(new Vector3(100, 200, 100));
        yield return new WaitForSeconds(7);

        rb.isKinematic = true;
        transform.position = startPos;
        transform.rotation = startRot;
        currentTile.taken = false;
        currentTile = tileZero;
        lastTile = tileZero;
    }

    private void OnDestroy()
    {
        Tile.OnTileSelected -= RegisterTileChoice;
        LaunchButton.OnTileEject -= EjectPressed;
    }
}
