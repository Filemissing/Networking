using System.Collections.Generic;
using UnityEngine;

public abstract class MovementStrategy : ScriptableObject
{
    protected Vector2Int[] orthogonal = new Vector2Int[4]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    protected Vector2Int[] diagonal = new Vector2Int[4]
    {
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1)
    };

    public abstract List<Vector2Int> CanMove(Vector2Int position, Piece piece);
    public abstract List<Vector2Int> CanTake(Vector2Int position, Piece piece);
}
