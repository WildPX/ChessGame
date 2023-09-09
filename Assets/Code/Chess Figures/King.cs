using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        // Right & Top right & Bottom right
        if (currentX + 1 < tileCountX) 
        {
            // Right
            if (board[currentX + 1, currentY] == null)
                result.Add(new Vector2Int(currentX + 1, currentY));
            else if (board[currentX + 1, currentY].team != team)
                result.Add(new Vector2Int(currentX + 1, currentY));

            // Top right
            if (currentY + 1 < tileCountY)
                if (board[currentX + 1, currentY + 1] == null)
                    result.Add(new Vector2Int(currentX + 1, currentY + 1));
                else if (board[currentX + 1, currentY + 1].team != team)
                    result.Add(new Vector2Int(currentX + 1, currentY + 1));

            // Bottom right
            if (currentY - 1 >= 0)
                if (board[currentX + 1, currentY - 1] == null)
                    result.Add(new Vector2Int(currentX + 1, currentY - 1));
                else if (board[currentX + 1, currentY - 1].team != team)
                    result.Add(new Vector2Int(currentX + 1, currentY - 1));
        }

        // Left
        if (currentX - 1 >= 0)
        {
            // Left
            if (board[currentX - 1, currentY] == null)
                result.Add(new Vector2Int(currentX - 1, currentY));
            else if (board[currentX - 1, currentY].team != team)
                result.Add(new Vector2Int(currentX - 1, currentY));

            // Top left
            if (currentY + 1 < tileCountY)
                if (board[currentX - 1, currentY + 1] == null)
                    result.Add(new Vector2Int(currentX - 1, currentY + 1));
                else if (board[currentX - 1, currentY + 1].team != team)
                    result.Add(new Vector2Int(currentX - 1, currentY + 1));

            // Bottom left
            if (currentY - 1 >= 0)
                if (board[currentX - 1, currentY - 1] == null)
                    result.Add(new Vector2Int(currentX - 1, currentY - 1));
                else if (board[currentX - 1, currentY - 1].team != team)
                    result.Add(new Vector2Int(currentX - 1, currentY - 1));
        }

        // Top
        if (currentY + 1 < tileCountY)
        {
            if (board[currentX, currentY + 1] == null || board[currentX, currentY + 1].team != team)
                result.Add(new Vector2Int(currentX, currentY + 1));
        }

        // Bottom
        if (currentY - 1 >= 0)
        {
            if (board[currentX, currentY - 1] == null || board[currentX, currentY - 1].team != team)
                result.Add(new Vector2Int(currentX, currentY - 1));
        }

        return result;
    }

    // Castling
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int ourY = (team == Team.White) ? 0 : 7;

        SpecialMove sm = SpecialMove.None;
        //Vector2Int[] kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));
        Vector2Int[] kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ourY);
        Vector2Int[] leftRook = moveList.Find(m => m[0].x == 0 && m[0].y == ourY);
        Vector2Int[] rightRook = moveList.Find(m => m[0].x == 7 && m[0].y == ourY);

        if (kingMove == null && currentX == 4)
        {
            //Left rook
            if (leftRook == null)
            {
                if (board[0, ourY].type == Type.Rook)
                    if (board[3, ourY] == null && board[2, ourY] == null && board[1, ourY] == null)
                    {
                        availableMoves.Add(new Vector2Int(2, ourY));
                        sm = SpecialMove.Castling;
                    }
            }
            // Right rook
            if (rightRook == null)
            {
                if (board[7, ourY].type == Type.Rook)
                    if (board[6, ourY] == null && board[5, ourY] == null)
                    {
                        availableMoves.Add(new Vector2Int(6, ourY));
                        sm = SpecialMove.Castling;
                    }
            }
        }

        return sm;
    }
}
