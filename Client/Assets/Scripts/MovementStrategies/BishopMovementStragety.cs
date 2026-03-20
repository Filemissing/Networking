using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BishopMovementStragety", menuName = "MovementStrategy/Bishop")]
public class BishopMovementStragety : MovementStrategy
{
    public List<Vector2Int> GetReachableTiles(Vector2Int position)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (Vector2Int direction in diagonal)
        {
            Vector2Int newPos = position + direction;

            do
            {
                if (!BoardManager.instance.IsValidCell(newPos))
                    break;

                result.Add(newPos);

                if (BoardManager.instance.ContainsPiece(newPos) != null)
                    break;

                newPos += direction;
            }
            while (true);
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
