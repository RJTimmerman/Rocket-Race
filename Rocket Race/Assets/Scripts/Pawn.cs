using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField] private Color colour;
    private const float moveDuration = 0.5f;
    private const float overtakeHeight = 2.5f;
    [HideInInspector] public bool inPlay = false;  // Only used to check if RegisterTileChoice should work

    private Tile currentTile;
    private List<Tile> nextTile;
    private Tile lastTile;  // This is being stored to make sure you cannot get back onto a safe spot you just came from
    private SkinnedMeshRenderer pawnRenderer;
    private MaterialPropertyBlock mpBlock;
    private Rigidbody rb;
    private Tile tileZero;

    private Vector3 beginPos;
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

        beginPos = transform.position;
        startRot = transform.rotation;
    }

    private void Start()
    {
        Tile.OnTileSelected += RegisterTileChoice;
        LaunchButton.OnTileEject += EjectPressed;
    }

    public IEnumerator Move(int steps, bool exactEnding)
    {
        bool reversed = false;  // To bounce back from the rocket if an exact ending is required
        
        for (int step = 0; step < steps; )
        {
            List<List<Tile>> options = new List<List<Tile>>();  // Can't easily copy LinkedList for new paths :(
            HashSet<Tile> checkedTiles = new HashSet<Tile> { lastTile, currentTile };

            void CheckStep(List<Tile> path, Tile tile)
            {
                if (checkedTiles.Contains(tile))
                    return;
                checkedTiles.Add(tile);
                
                List<Tile> newPath = path.ToList();
                newPath.Add(tile);
                
                if (!tile.taken)
                    options.Add(newPath);
                else if (!tile.halt)
                    foreach (Tile child in reversed ? tile.prevTiles : tile.nextTiles)
                        CheckStep(newPath, child);
            }
            foreach (Tile child in reversed ? currentTile.prevTiles : currentTile.nextTiles)
                CheckStep(new List<Tile>(), child);

            switch (options.Count)
            {
                case 0:
                    throw new Exception("No valid tiles to move to!");  // This should only happen if the board isn't well-defined
                case 1:
                    nextTile = options[0];
                    break;
                default:
                    foreach (List<Tile> tile in options)
                        tile.Last().Highlight(tile);

                    yield return new WaitWhile(() => nextTile is null);

                    foreach (List<Tile> tile in options)
                        tile.Last().Unhighlight();
                    break;
            }
            
            yield return MoveToNextTile();

            step += nextTile.Count;
            
            lastTile = currentTile;
            currentTile = nextTile.Last();
            nextTile = null;
            lastTile.taken = false;
            currentTile.taken = true;

            if (currentTile.halt)
                break;

            if (currentTile.colour == Tile.TileColour.Rocket)
            {
                if (!exactEnding || step >= steps)
                    yield return Win();
                else
                {
                    reversed = true;
                    lastTile = null;
                }
            }
        }

        lastTile = null;
    }

    private IEnumerator MoveToNextTile()
    {
        int pathLength = nextTile.Count;
        
        if (pathLength == 1)
            yield return MTTHop(nextTile[0].transform.position);
        else
        {
            yield return MTTFirst(nextTile[0].transform.position);

            for (int i = 1; i < pathLength - 1; i++)
                yield return MTTMiddle(nextTile[i].transform.position);

            yield return MTTLast(nextTile[pathLength - 1].transform.position);
        }
    }
    
    private IEnumerator MTTHop(Vector3 targetPos)
    {
        float timePassed = 0;
        Vector3 startPos = transform.position;
        
        while (timePassed < moveDuration)
        {
            timePassed += Time.deltaTime;
            transform.position = startPos + timePassed / moveDuration * (targetPos - startPos);
            transform.Translate(0, -(2 * overtakeHeight / (moveDuration * moveDuration)) * (timePassed - moveDuration / 2) * (timePassed - moveDuration / 2) + overtakeHeight / 2, 0);
            yield return null;
        }
        transform.position = targetPos;
    }
    
    private IEnumerator MTTFirst(Vector3 targetPos)
    {
        float timePassed = 0;
        Vector3 startPos = transform.position;
        
        while (timePassed < moveDuration)
        {
            timePassed += Time.deltaTime;
            transform.position = startPos + timePassed / moveDuration * (targetPos - startPos);
            transform.Translate(0, -(overtakeHeight/(moveDuration*moveDuration)) * (timePassed - moveDuration) * (timePassed - moveDuration) + overtakeHeight, 0);
            yield return null;
        }
        transform.position = targetPos + new Vector3(0, overtakeHeight, 0);
    }
    
    private IEnumerator MTTMiddle(Vector3 targetPos)
    {
        float timePassed = 0;
        Vector3 startPos = transform.position;
        targetPos += new Vector3(0, overtakeHeight, 0);
        
        while (timePassed < moveDuration)
        {
            timePassed += Time.deltaTime;
            transform.position = startPos + timePassed / moveDuration * (targetPos - startPos);
            yield return null;
        }
        transform.position = targetPos;
    }
    
    private IEnumerator MTTLast(Vector3 targetPos)
    {
        float timePassed = 0;
        Vector3 startPos = transform.position - new Vector3(0, overtakeHeight, 0);

        while (timePassed < moveDuration)
        {
            timePassed += Time.deltaTime;
            transform.position = startPos + timePassed / moveDuration * (targetPos - startPos);
            transform.Translate(0, -(overtakeHeight / (moveDuration * moveDuration)) * (timePassed * timePassed) + overtakeHeight, 0);
            yield return null;
        }
        transform.position = targetPos;
    }

    private void RegisterTileChoice(List<Tile> tile)
    {
        if (inPlay)
            nextTile = tile;
    }

    private void EjectPressed(Tile.TileColour tileColour, bool _)
    {
        if (currentTile.colour == tileColour)
            StartCoroutine( GetEjected() );
    }

    private IEnumerator GetEjected()
    {
        rb.isKinematic = false;
        rb.AddForce(new Vector3(200, 700, -200));
        rb.AddTorque(new Vector3(100, 200, 100));
        yield return new WaitForSeconds(4);

        rb.isKinematic = true;
        transform.position = beginPos;
        transform.rotation = startRot;
        currentTile.taken = false;
        currentTile = tileZero;
        lastTile = tileZero;
    }

    private IEnumerator Win()
    {
        throw new NotImplementedException(name + " won!");
    }

    private void OnDestroy()
    {
        Tile.OnTileSelected -= RegisterTileChoice;
        LaunchButton.OnTileEject -= EjectPressed;
    }
}
