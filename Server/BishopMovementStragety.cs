using System.Collections.Generic;
using System.Linq;

public class BishopMovementStragety : MovementStrategy
{
    public List<Vector2Int> GetReachableTiles(Vector2Int position, Game game)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int direction in diagonal)
        {
            Vector2Int newPos = position + direction;

            do
            {
                if (!game.IsValidCell(newPos))
                    break;

                result.Add(newPos);

                if (game.ContainsPiece(newPos) != null)
                    break;

                newPos += direction;
            }
            while (true);
        }

        return result;
    }

    public override List<Vector2Int> CanMove(Vector2Int position, Piece piece, Game game)
    {
        return GetReachableTiles(position, game).Where(p => !(game.ContainsPiece(p) != null)).ToList();
    }
    public override List<Vector2Int> CanTake(Vector2Int position, Piece piece, Game game, Color playerColor)
    {
        return GetReachableTiles(position, game).Where(p => { Piece? piece = game.ContainsPiece(p); return piece != null && piece.color != playerColor; }).ToList();
    }
}
