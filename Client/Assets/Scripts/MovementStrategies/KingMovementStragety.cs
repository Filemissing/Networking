using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "KingMovementStragety", menuName = "MovementStrategy/King")]
public class KingMovementStragety : MovementStrategy
{
    public List<Vector2Int> GetReachableTiles(Vector2Int position)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int direction in orthogonal)
        {
            Vector2Int newPos = position + direction;
            if (!BoardManager.instance.IsValidCell(newPos))
                continue;

            result.Add(position + direction);
        }

        foreach (Vector2Int direction in diagonal)
        {
            Vector2Int newPos = position + direction;
            if (!BoardManager.instance.IsValidCell(newPos))
                continue;

            result.Add(position + direction);
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
