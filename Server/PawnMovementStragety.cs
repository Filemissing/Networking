using System.Collections.Generic;
using System.Linq;

public class PawnMovementStragety : MovementStrategy
{
    public override List<Vector2Int> CanMove(Vector2Int position, Piece piece, Game game)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Vector2Int forward = piece.color == Color.White ? Vector2Int.up : Vector2Int.down;
        Vector2Int oneStep = position + forward;

        // Can't move forward if blocked
        if (game.ContainsPiece(oneStep) != null)
            return result;

        result.Add(oneStep);

        // Double move only on first move and only if the square beyond is also clear
        if (piece.firstMove)
        {
            Vector2Int twoStep = oneStep + forward;
            if (game.ContainsPiece(twoStep) == null)
                result.Add(twoStep);
        }

        return result;
    }

    public override List<Vector2Int> CanTake(Vector2Int position, Piece piece, Game game, Color playerColor)
    {
        int dir = piece.color == Color.White ? 1 : -1;
        List<Vector2Int> result = new List<Vector2Int>();
        Vector2Int rightDiagonal = new Vector2Int(position.x + 1, position.y + dir);
        Vector2Int leftDiagonal = new Vector2Int(position.x - 1, position.y + dir);

        if (game.IsValidCell(rightDiagonal))
            result.Add(rightDiagonal);
        if (game.IsValidCell(leftDiagonal))
            result.Add(leftDiagonal);
        
        return result.Where(p => { Piece? target = game.ContainsPiece(p); return target != null && target.color != piece.color; }).ToList();
    }
}
