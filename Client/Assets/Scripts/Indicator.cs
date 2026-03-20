using UnityEngine;
using UnityEngine.EventSystems;

public class Indicator : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        BoardManager.instance.MoveSelectedPiece(BoardManager.instance.GetCellFromUICoords(this.transform as RectTransform));
        BoardManager.instance.selectedPiece = null;
        BoardManager.instance.ClearIndicators();
    }
}
