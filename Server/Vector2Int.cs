using System.Reflection.Metadata.Ecma335;

public class Vector2Int
{
    public int x, y;

    public static readonly Vector2Int up = new Vector2Int(0, 1);
    public static readonly Vector2Int down = new Vector2Int(0, -1);

    public Vector2Int(int x, int y)
    {
        this.x = x; 
        this.y = y;
    }

    public static Vector2Int operator +(Vector2Int left, Vector2Int right) =>
        new Vector2Int(left.x + right.x, left.y + right.y);

    public static bool operator ==(Vector2Int left, Vector2Int right) =>
        left.x == right.x && left.y == right.y;

    public static bool operator !=(Vector2Int left, Vector2Int right) =>
        !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is Vector2Int other)
            return x == other.x && y == other.y;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y);
    }
}
