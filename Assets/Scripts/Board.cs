using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour

{
    [SerializeField] int height, width;
    public GameObject tileObject;
    public float cameraVerticalOffset,cameraSizeOffset;
    public GameObject[] availablePieces;
    Tile[,] tiles;
    Pieces[,] pieces;
    Tile startTile, endTile;
    // Start is called before the first frame update
    void Start()
    {
        tiles = new Tile[width, height];
        pieces= new Pieces[width, height];
        SetupBoard();
        PositionCamera();
        SetupPieces();
    }

    private void SetupPieces()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var selectedpiece = availablePieces[UnityEngine.Random.Range(0, availablePieces.Length)];
                var o = Instantiate(selectedpiece, new Vector3(x, y - 5), Quaternion.identity);
                o.transform.parent = transform;
                pieces[x, y] = o.GetComponent<Pieces>();
                pieces[x, y] .Setup(x, y, this);
            }
        }
    }

    private void PositionCamera()
    {
        float newPosX =(float) width/2f;
        float newPosY = (float)height / 2f;
        Camera.main.transform.position=new Vector3 (newPosX -0.5f,newPosY-0.5f + cameraVerticalOffset, -10f);
        float horizontal = width + 1;
        float vertical = (height /2)+1;
        Camera.main.orthographicSize = horizontal > vertical ? horizontal + cameraSizeOffset : vertical + cameraSizeOffset;
    }

    private void SetupBoard()
    {
       for (int x = 0; x < width; x++) 
        {
        for(int y = 0; y < height; y++)
            {
                var o = Instantiate(tileObject, new Vector3(x, y - 5), Quaternion.identity);
                o.transform.parent = transform;
                tiles[x, y] = o.GetComponent<Tile>();
                tiles[x, y]?.Setup(x, y, this);
            }
        }
    }
    public void TileDown(Tile tile_)
    {
       startTile=tile_;
    }
    public void TileOver(Tile tile_)
    {
        endTile=tile_;
    }
    public void TileUp(Tile tile_)
    {
        if (startTile != null && endTile != null)
        {
            SwapTiles();
        }
        startTile = null;
        endTile = null;
    }

    private void SwapTiles()
    {
        var StarPiece = pieces[startTile.x, startTile.y];
        var EndPiece = pieces[endTile.x, endTile.y];
        StarPiece.Move(endTile.x, endTile.y);
        EndPiece.Move(startTile.x, startTile.y);

        pieces[startTile.x, startTile.y]= EndPiece;
        pieces[endTile.x, endTile.y]= StarPiece;
    }
}
