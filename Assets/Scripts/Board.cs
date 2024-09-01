using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour

{
    public float timeBetweenPieces = 0.01f;
    [SerializeField] int height, width;
    public GameObject tileObject;
    public float cameraVerticalOffset, cameraSizeOffset;
    public GameObject[] availablePieces;
    Tile[,] tiles;
    Pieces[,] pieces;
    Tile startTile, endTile;
    public int PointsPerMatch;
    bool swappingPieces = false;
    // Start is called before the first frame update
    void Start()
    {
        tiles = new Tile[width, height];
        pieces = new Pieces[width, height];
        SetupBoard();
        PositionCamera();
        StartCoroutine(SetupPieces());
    }

    private IEnumerator SetupPieces()
    {
        int maxIterations = 50;
        int currentIteration = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                yield return new WaitForSeconds(timeBetweenPieces);
                if (pieces[x,y] == null)
                {
                    currentIteration = 0;
                    var newPiece = CreatePieceAt(x, y);
                    while (HasPreviousMatches(x, y))
                    {
                        ClearPieceAt(x, y);
                        newPiece = CreatePieceAt(x, y);
                        currentIteration++;
                        if (currentIteration > maxIterations)
                        {
                            break;
                        }
                    }
                }
            }
        }
        yield return null;
    }

    private void ClearPieceAt(int x, int y)
    {
        var pieceToClear = pieces[x, y];
        pieceToClear.Remove(true);
        pieces[x, y] = null;
       
    }

    private Pieces CreatePieceAt(int x, int y)
    {
        var selectedpiece = availablePieces[UnityEngine.Random.Range(0, availablePieces.Length)];
        var o = Instantiate(selectedpiece, new Vector3(x, y +1 - 5), Quaternion.identity);
        o.transform.parent = transform;
        pieces[x, y] = o.GetComponent<Pieces>();
        pieces[x, y].Setup(x, y, this);
        pieces[x, y].Move(x, y);
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
        if (!swappingPieces)
        {
            startTile = tile_;
        }
    }
    public void TileOver(Tile tile_)
    {
        if (!swappingPieces)
        {
            endTile = tile_;
        }
    }
    public void TileUp(Tile tile_)
    {

        if (!swappingPieces)
        {
            if (startTile != null && endTile != null && IsCloseTo(startTile, endTile))
            {
                StartCoroutine(SwapTiles());
            }
        }
    }

    IEnumerator SwapTiles()
    {
        swappingPieces = true;
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
            AwardPoints(allMatches);
        }

        startTile = null; endTile=null; yield return null;
    }

    private void ClearPieces(List<Pieces> piecesToClear)
    {
        piecesToClear.ForEach(piece =>
        {
            ClearPieceAt(piece.x, piece.y);
        });
        List <int> columns = GetColumns (piecesToClear);
        List <Pieces> collapsePieces = collapseColumns(columns, 0.3f);
        FindMatchsRecusrsively(collapsePieces);
    }

    private void FindMatchsRecusrsively(List<Pieces> collapsePieces)
    {
        StartCoroutine(FindMatchsRecusrsivelyCoroutine(collapsePieces));
    }

    IEnumerator FindMatchsRecusrsivelyCoroutine(List<Pieces> collapsePieces)
    {
        yield return new WaitForSeconds(1f);
        List<Pieces> newMatches = new List<Pieces>();
        collapsePieces.ForEach(piece =>
        {
            var matches = GetMatchByPiece(piece.x, piece.y, 3);
            if (matches != null)
            {
               newMatches = newMatches.Union(matches).ToList();
               ClearPieces(matches);
               AwardPoints (newMatches);
            }
        });
        if (newMatches.Count > 0)
        {
            var newCollapsePieces = collapseColumns(GetColumns(newMatches), 0.3f);
            FindMatchsRecusrsively(newCollapsePieces);
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(SetupPieces());
            swappingPieces = false;
        }
        yield return null;
    }

    private List<int> GetColumns (List<Pieces> piecesToClear)
    {
        var result = new List<int>();
        piecesToClear.ForEach(piece =>
        {
            if (!result.Contains(piece.x))
            {
                result.Add(piece.x);
            }
        });
        return result;
    }
    private List<Pieces> collapseColumns(List<int> columns, float timeToCollapse)
    {
        List<Pieces> movingPieces = new List<Pieces>();
        for (int i = 0; i< columns.Count; i++)
        {
            var column = columns[i];
            for (int y = 0; y< height; y++)
            {
                if (pieces[column, y] == null)
                {
                    for (int yplus = y+1; yplus < height; yplus++)
                    {
                        if (pieces[column, yplus] == null)
                        {
                            pieces[column, yplus].Move(column, y);
                            pieces[column, y] = pieces[column, yplus];
                            if (!movingPieces.Contains(pieces[column, y]))
                            {
                                movingPieces.Add(pieces[column, y]);
                            }
                            pieces[column, yplus] = null;
                            break;
                        }
                    }
                }
            }
        }
        return movingPieces;
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
    public void AwardPoints(List<Pieces> allMatches)
    {
        GameManager.Instance.Addpoint(allMatches.Count*PointsPerMatch);
    }
}

