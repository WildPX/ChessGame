using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    White = 0,
    Black = 1
}

public enum Type
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public class ChessPiece : MonoBehaviour
{
    // White or black team. False = white, true = black
    public Team team;
    // Positions
    public int currentX, currentY;
    // Type of a chess piece
    public Type type;

    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));

        return r;
    }

    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        return SpecialMove.None;
    }
    /*
    private Vector2 desiredPosition;
    private Vector2 desiredScale = Vector2.one;

    private void Update()
    {
        {
            transform.position = Vector2.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
            transform.localScale = Vector2.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
        }
    }

    public virtual void SetPosition(Vector2 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
            transform.position = desiredPosition;
    }

    public virtual void SetScale(Vector2 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
            transform.localScale = desiredScale;
    }
    */
}
