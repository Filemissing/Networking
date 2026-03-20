using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PawnMovementStragety", menuName = "MovementStrategy/Pawn")]
public class PawnMovementStragety : MovementStrategy
{
    public override List<Vector2Int> CanMove(Vector2Int position, Piece piece)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Vector2Int forward = piece.color == Color.White ? Vector2Int.up : Vector2Int.down;
        Vector2Int oneStep = position + forward;

        // Can't move forward if blocked
        if (BoardManager.instance.ContainsPiece(oneStep) != null)
            return result;

        result.Add(oneStep);

        // Double move only on first move and only if the square beyond is also clear
        if (piece.firstMove)
        {
            Vector2Int twoStep = oneStep + forward;
            if (BoardManager.instance.ContainsPiece(twoStep) == null)
                result.Add(twoStep);
        }

        return result;
    }

    public override List<Vector2Int> CanTake(Vector2Int position, Piece piece)
    {
        int dir = piece.color == Color.White ? 1 : -1;
        List<Vector2Int> result = new List<Vector2Int>();
        Vector2Int rightDiagonal = new Vector2Int(position.x + 1, position.y + dir);
        Vector2Int leftDiagonal = new Vector2Int(position.x - 1, position.y + dir);

        if (BoardManager.instance.IsValidCell(rightDiagonal))
            result.Add(rightDiagonal);
        if (BoardManager.instance.IsValidCell(leftDiagonal))
            result.Add(leftDiagonal);
        
        return result.Where(p => { Piece target = BoardManager.instance.ContainsPiece(p); return target != null && target.color != piece.color; }).ToList();
    }
}
