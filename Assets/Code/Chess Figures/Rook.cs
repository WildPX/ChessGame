using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        // Move down
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (board[currentX, i] == null)
                result.Add(new Vector2Int(currentX, i));

            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team)
                    result.Add(new Vector2Int(currentX, i));

                break;
            }
        }
        // Move top
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (board[currentX, i] == null)
                result.Add(new Vector2Int(currentX, i));

            if (board[currentX, i] != null)
            {
                if (board[currentX, i].team != team)
                    result.Add(new Vector2Int(currentX, i));

                break;
            }
        }
        // Move right
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (board[i, currentY] == null)
                result.Add(new Vector2Int(i, currentY));

            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team)
                    result.Add(new Vector2Int(i, currentY));

                break;
            }
        }
        // Move left
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (board[i, currentY] == null)
                result.Add(new Vector2Int(i, currentY));

            if (board[i, currentY] != null)
            {
                if (board[i, currentY].team != team)
                    result.Add(new Vector2Int(i, currentY));

                break;
            }
        }

        return result;
    }
}
