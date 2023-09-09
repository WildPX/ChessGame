using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        int move = (team == Team.White) ? 1 : -1;

        // One in front
        if (board[currentX, currentY + move] == null)
            result.Add(new Vector2Int(currentX, currentY + move));

        // Two in front
        if (board[currentX, currentY + move] == null)
        {
            if (team == Team.White && currentY == 1 && board[currentX, currentY + move * 2] == null)
                result.Add(new Vector2Int(currentX, currentY + move * 2));
            else if (team == Team.Black && currentY == 6 && board[currentX, currentY + move * 2] == null)
                result.Add(new Vector2Int(currentX, currentY + move * 2));
        }

        // Kill chess piece (diagonal)
        // for the right side
        if (currentX != tileCountX - 1) // Doing so we won't end up outside of the board
            if (board[currentX + 1, currentY + move] != null && board[currentX + 1, currentY + move].team != team)
                result.Add(new Vector2Int(currentX + 1, currentY + move));
        // left side
        if (currentX != 0) // Same thing
            if (board[currentX - 1, currentY + move] != null && board[currentX - 1, currentY + move].team != team)
                result.Add(new Vector2Int(currentX - 1, currentY + move));

        return result;
    }

    // En passant & Promotion
    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int move = (team == Team.White) ? 1 : -1;

        // Promotion
        if (team == Team.White && currentY == 6 || team == Team.Black && currentY == 1)
            return SpecialMove.Promotion;

        // En passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if (board[lastMove[1].x, lastMove[1].y].type == Type.Pawn) // if the last piece = pawn
            {
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2) // Two up (down) move
                {
                    if (board[lastMove[1].x, lastMove[1].y].team != team)
                    {
                        if (lastMove[1].y == currentY) // If both pawns are on the same square
                        {
                            if (lastMove[1].x == currentX - 1) // Landed on the left
                            {
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + move));
                                return SpecialMove.EnPassant;
                            }
                            else if (lastMove[1].x == currentX + 1) // Landed on the right
                            {
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + move));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }
            }
        }

        return SpecialMove.None;
    }
}
