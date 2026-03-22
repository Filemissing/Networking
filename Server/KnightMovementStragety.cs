using System.Collections.Generic;
using System.Linq;

public class KnightMovementStragety : MovementStrategy
{
    public bool[,] kernel = new bool[5, 5]
    {
        { false, true, false, true, false },
        { true, false, false, false, true },
        { false, false, false, false, false },
        { true, false, false, false, true },
        { false, true, false, true, false }
    };

    public List<Vector2Int> GetReachableTiles(Vector2Int position, Game game)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int x = -2; x <= 2; x++)
            for (int y = -2; y <= 2; y++)
                if (kernel[x + 2, y + 2] == true)
                {
                    Vector2Int newPos = new Vector2Int(position.x + x, position.y + y);
                    if (game.IsValidCell(newPos))
                        result.Add(newPos);
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
