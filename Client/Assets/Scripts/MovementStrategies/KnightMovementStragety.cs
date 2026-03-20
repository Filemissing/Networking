using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "KnightMovementStragety", menuName = "MovementStrategy/Knight")]
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

    public List<Vector2Int> GetReachableTiles(Vector2Int position)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int x = -2; x <= 2; x++)
            for (int y = -2; y <= 2; y++)
                if (kernel[x + 2, y + 2] == true)
                {
                    Vector2Int newPos = new Vector2Int(position.x + x, position.y + y);
                    if (BoardManager.instance.IsValidCell(newPos))
                        result.Add(newPos);
                }

        return result;
    }

    public override List<Vector2Int> CanMove(Vector2Int position, Piece piece)
    {
        return GetReachableTiles(position).Where(p => !BoardManager.instance.ContainsPiece(p)).ToList();
    }
    public override List<Vector2Int> CanTake(Vector2Int position, Piece piece)
    {
        return GetReachableTiles(position).Where(p => { Piece piece = BoardManager.instance.ContainsPiece(p); return piece != null && piece.color != BoardManager.instance.playerColor; }).ToList();
    }
}
