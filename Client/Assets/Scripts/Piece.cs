using UnityEngine;
using UnityEngine.EventSystems;

public class Piece : MonoBehaviour, IPointerClickHandler
{
    public Color color;
    public PieceType type;
    public MovementStrategy strategy;
    public bool firstMove = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (color != BoardManager.instance.playerColor)
            return;
        BoardManager.instance.selectedPiece = this;
        BoardManager.instance.ClearIndicators();
        BoardManager.instance.ShowIndicators(this);
    }
}

public enum PieceType
{
    King,
    Queen,
    Bishop,
    Knight,
    Rook,
    Pawn
}

public enum Color
{
    White, 
    Black
}
