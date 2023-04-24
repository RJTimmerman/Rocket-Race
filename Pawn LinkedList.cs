using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    [SerializeField] private Color colour;
    private const float moveDuration = 1;
    private const float overtakeHeight = 2.5f;

    private Tile currentTile;
    private LinkedList<Tile> nextTile;
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
    
    public IEnumerator Move(int steps)
    {
        for (int step = 0; step < steps; )
        {
            List<LinkedList<Tile>> options = new List<LinkedList<Tile>>();
            HashSet<Tile> checkedTiles = new HashSet<Tile> { lastTile };

            void CheckStep(LinkedList<Tile> tilePath)
            {
                foreach (Tile option in tilePath.Last.Value.nextTiles)
                {
                    if (checkedTiles.Contains(option))
                        return;
                    
                    if (!option.taken)
                        options.Add(new LinkedList<Tile>().AddFirst(option).List);
                    else
                        CheckStep(tilePath.AddLast(option));
                }
            }
            
            
            
            // foreach (Tile option in currentTile.nextTiles)
            // {
            //     if (option == lastTile)
            //         continue;
            //     
            //     if (!option.taken)
            //         options.Add(new LinkedList<Tile>().AddFirst(option).List);
            //     else
            //     {
            //         
            //     }
            // }

            switch (options.Count)
            {
                case 0:
                    throw new Exception("No valid tiles to move to!");  // This should only happen if the board isn't well-defined
                // break;
                case 1:
                    nextTile = options[0];
                    break;
                default:
                    foreach (LinkedList<Tile> tile in options)
                        tile.Last.Value.Highlight(tile);

                    yield return new WaitWhile(() => nextTile is null);

                    foreach (LinkedList<Tile> tile in options)
                        tile.Last.Value.Unhighlight();
                    break;
            }
            
            yield return MoveToNextTile();

            step += nextTile.Count;
            
            lastTile = currentTile;
            currentTile = nextTile.Last.Value;
            nextTile = null;
            lastTile.taken = false;
            currentTile.taken = true;

            if (currentTile.halt)
                break;
        }
    }

    /*private IEnumerator MoveTo(Vector3 targetPos, float duration)
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
    }*/

    private IEnumerator MoveToNextTile()
    {
        int pathLength = nextTile.Count;
        
        if (pathLength == 1)
            yield return MTTHop();
        else
        {
            yield return MTTFirst(nextTile.First.Value.transform.position);

            for (LinkedListNode<Tile> tile = nextTile.First.Next; tile != nextTile.Last; tile = tile.Next)
                yield return MTTMiddle(tile.Value.transform.position);

            yield return MTTLast(nextTile.Last.Value.transform.position);
        }
    }
    
    private IEnumerator MTTHop()
    {
        float timePassed = 0;
        Vector3 startPos = transform.position;
        Vector3 targetPos = nextTile.Last.Value.transform.position;
        
        while (timePassed < moveDuration)
        {
            timePassed += Time.deltaTime;
            transform.position = startPos + timePassed * (targetPos - startPos);
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
            transform.position = startPos + timePassed * (targetPos - startPos);
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
            transform.position = startPos + timePassed * (targetPos - startPos);
            yield return null;
        }
        transform.position = targetPos;
    }
    
    private IEnumerator MTTLast(Vector3 targetPos)
    {
        float timePassed = 0;
        Vector3 startPos = transform.position;

        while (timePassed < moveDuration)
        {
            timePassed += Time.deltaTime;
            transform.position = startPos + timePassed * (targetPos - startPos);
            transform.Translate(0, -(overtakeHeight / (moveDuration * moveDuration)) * (timePassed * timePassed), 0);
            yield return null;
        }
        transform.position = targetPos;
    }

    private void RegisterTileChoice(LinkedList<Tile> tile)
    {
        nextTile = tile;
    }

    private void EjectPressed(Tile.TileColour tileColour)
    {
        if (currentTile.colour != tileColour)
            return;
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

    private void OnDestroy()
    {
        Tile.OnTileSelected -= RegisterTileChoice;
        LaunchButton.OnTileEject -= EjectPressed;
    }
}
