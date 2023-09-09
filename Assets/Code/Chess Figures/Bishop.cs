using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        // Top right
        for (int i = currentX + 1, j = currentY + 1; i < tileCountX && j < tileCountY; i++, j++) {
            if (board[i, j] == null)
                result.Add(new Vector2Int(i, j));
            else {
                if (board[i, j].team != team)
                    result.Add(new Vector2Int(i, j));

                break;
            }
        }

        // Top left
        for (int i = currentX - 1, j = currentY + 1; i >= 0 && j < tileCountY; i--, j++)
        {
            if (board[i, j] == null)
                result.Add(new Vector2Int(i, j));
            else
            {
                if (board[i, j].team != team)
                    result.Add(new Vector2Int(i, j));

                break;
            }
        }

        // Bottom right
        for (int i = currentX + 1, j = currentY - 1; i < tileCountX && j >= 0; i++, j--)
        {
            if (board[i, j] == null)
                result.Add(new Vector2Int(i, j));
            else
            {
                if (board[i, j].team != team)
                    result.Add(new Vector2Int(i, j));

                break;
            }
        }

        // Bottom left
        for (int i = currentX - 1, j = currentY - 1; i >= 0 && j >= 0; i--, j--)
        {
            if (board[i, j] == null)
                result.Add(new Vector2Int(i, j));
            else
            {
                if (board[i, j].team != team)
                    result.Add(new Vector2Int(i, j));

                break;
            }
        }

        return result;
    }
}