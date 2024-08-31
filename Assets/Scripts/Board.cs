using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour

{
    [SerializeField] int height, width;
    public GameObject tileObject;
    public float cameraVerticalOffset, cameraSizeOffset;
    public GameObject[] availablePieces;
    Tile[,] tiles;
    Pieces[,] pieces;
    Tile startTile, endTile;

    bool swappingPieces = false;
    // Start is called before the first frame update
    void Start()
    {
        tiles = new Tile[width, height];
        pieces = new Pieces[width, height];
        SetupBoard();
        PositionCamera();
        SetupPieces();
    }

    private void SetupPieces()
    {
        int maxIterations = 50;
        int currentIteration = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                currentIteration = 0;
                var newPiece = CreatePieceAt(x, y);
                while (HasPreviousMatches(x, y)) 
                {
                    ClearPieceAt(x, y);
                    newPiece= CreatePieceAt(x, y);
                    currentIteration++;
                    if (currentIteration > maxIterations)
                    {
                        break;
                    }

                }
            }
        }
    }

    private void ClearPieceAt(int x, int y)
    {
        var pieceToClear = pieces[x, y];
        Destroy(pieceToClear.gameObject);
        pieces[x, y] = null;
       
    }

    private Pieces CreatePieceAt(int x, int y)
    {
        var selectedpiece = availablePieces[UnityEngine.Random.Range(0, availablePieces.Length)];
        var o = Instantiate(selectedpiece, new Vector3(x, y - 5), Quaternion.identity);
        o.transform.parent = transform;
        pieces[x, y] = o.GetComponent<Pieces>();
        pieces[x, y].Setup(x, y, this);
        return pieces[x, y];
    }
    private void PositionCamera()
    {
        float newPosX = (float)width / 2f;
        float newPosY = (float)height / 2f;
        Camera.main.transform.position = new Vector3(newPosX - 0.5f, newPosY - 0.5f + cameraVerticalOffset, -10f);
        float horizontal = width + 1;
        float vertical = (height / 2) + 1;
        Camera.main.orthographicSize = horizontal > vertical ? horizontal + cameraSizeOffset : vertical + cameraSizeOffset;
    }

    private void SetupBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
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
        startTile = tile_;
    }
    public void TileOver(Tile tile_)
    {
        endTile = tile_;
    }
    public void TileUp(Tile tile_)
    {
        if (startTile != null && endTile != null && IsCloseTo(startTile, endTile))
        {
            StartCoroutine(SwapTiles());
        }
        
    }

    IEnumerator SwapTiles()
    {
        var StartPiece = pieces[startTile.x, startTile.y];
        var EndPiece = pieces[endTile.x, endTile.y];
        StartPiece.Move(endTile.x, endTile.y);
        EndPiece.Move(startTile.x, startTile.y);

        pieces[startTile.x, startTile.y] = EndPiece;
        pieces[endTile.x, endTile.y] = StartPiece;

        yield return new WaitForSeconds(0.6f);

        var startMatches = GetMatchByPiece(startTile.x, startTile.y, 3);
        var endMatches = GetMatchByPiece(endTile.x,endTile.y, 3);

        var allMatches = startMatches.Union(endMatches).ToList();
         
        
        if (allMatches.Count ==0)
        {
            StartPiece.Move(startTile.x, startTile.y);
            EndPiece.Move(endTile.x, endTile.y);
            pieces[startTile.x, startTile.y] = StartPiece;
            pieces[endTile.x, endTile.y] = EndPiece;
        }

        else
        {
            ClearPieces(allMatches);
        }

        startTile = null; endTile=null; yield return null;
    }

    private void ClearPieces(List<Pieces> piecesToClear)
    {
        piecesToClear.ForEach(piece =>
        {
            ClearPieceAt(piece.x, piece.y);
        });
    }

    public bool IsCloseTo(Tile start, Tile end)
    {
        if (Math.Abs(start.x - end.x) == 1 && start.y == end.y)
        {
            return true;
        }
        if (Math.Abs(start.y - end.y) == 1 && start.x == end.x)
        {
            return true;
        }
        return false;
    }
     bool HasPreviousMatches(int posX, int posY)
    {
        var downMatch = GetMatchByDirection(posX, posY, new Vector2(0, -1), 2);
        var leftMatch = GetMatchByDirection(posX, posY, new Vector2(-1,0), 2);

        if (downMatch != null) downMatch = new List<Pieces>();
        if (leftMatch != null) leftMatch = new List<Pieces>();
        return(downMatch.Count > 0) || leftMatch.Count > 0;
    }

    public List<Pieces> GetMatchByDirection(int xpos, int ypos, Vector2 direction, int minPieces = 3)
    {
        List<Pieces> matches = new List<Pieces>();
        Pieces startPiece = pieces[xpos, ypos];
        matches.Add(startPiece);

        int nextX, nextY;
        int maxVal = width > height ? width : height;
        for (int i = 0; i < maxVal; i++)
        {
            nextX = xpos + ((int)direction.x * i);
            nextY = ypos + ((int)direction.y * i);
            if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height)
            {
                var nextPiece = pieces[nextX, nextY];
                if (nextPiece != null && nextPiece.pieceType == startPiece.pieceType)
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }
        }
        if (matches.Count >= minPieces)

        {
            return matches;
        }
        return null;
    }

    
    public List<Pieces> GetMatchByPiece(int xpos, int ypos, int minPieces = 3)
    {
        var upMatch = GetMatchByDirection(xpos, ypos, new Vector2(0,1), 2);
        var downMatch = GetMatchByDirection(xpos, ypos, new Vector2(0, -1), 2);
        var rightMatch = GetMatchByDirection(xpos, ypos, new Vector2(1, 0), 2);
        var leftMatch = GetMatchByDirection(xpos, ypos, new Vector2(-1, 0), 2);

        if (upMatch == null) upMatch = new List<Pieces>();
        if (downMatch == null) downMatch = new List<Pieces>();
        if (rightMatch == null) rightMatch = new List<Pieces>();
        if (leftMatch == null) leftMatch = new List<Pieces>();

        var verticalMatches = upMatch.Union(downMatch).ToList();
        var horizontalMatches = leftMatch.Union(rightMatch).ToList();

        var foundMatches = new List<Pieces>();

        if (verticalMatches.Count >= minPieces)
        {
            foundMatches = foundMatches.Union(verticalMatches).ToList();
        }
        if (horizontalMatches.Count >= minPieces)
        {
            foundMatches = foundMatches.Union(horizontalMatches).ToList();
        }
        return foundMatches;
    }
}

