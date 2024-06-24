using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Pieces : MonoBehaviour
{
    public int x, y;
    public Board board;

    public enum type 
    { 
        elephant,
        giraffe,
        hippo,
        monkey,
        panda,
        parrot,
        penguin,
        pig,
        rabbit,
        snake
    };
    public type pieceType;
    public void Setup(int x_, int y_, Board board_)
    {
        x = x_;
        y = y_;
        board = board_;
    }
    public void Move(int destX, int destY)
    {
        transform.DOMove(new Vector3(destX, destY, -5f), 0.25f).SetEase(Ease.InOutCubic).onComplete = () =>
        {
            x = destX; y = destY;
        };
    }

}
