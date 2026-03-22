using Rug.Osc;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
    }

    CanvasGroup canvasGroup;
    public void HandleGameEvent(OscMessage msg, string subAddress)
    {
        switch (subAddress)
        {
            case "start":
                StartGame((Color)Enum.Parse(typeof(Color), msg[0] as string, true));
                break;

            case "stop":
                EndGame((EndState)Enum.Parse(typeof(EndState), msg[0] as string, true));
                break;
        }
    }
    public void StartGame(Color playerColor)
    {
        DisableGameEndCover();

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        this.playerColor = playerColor;
        InitializeBoard();
    }
    public void EndGame(EndState endState)
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        EnableGameEndCover(endState);

    }

    Piece[,] board = new Piece[8, 8];
    [SerializeField] Transform piecesParent;
    void InitializeBoard()
    {
        ClearBoard();
        // (file, rank) => piece prefab, indexed [x][y] matching your board layout
        Piece[,] prefabs = new Piece[8, 8]
        {
        // x=0 (a-file)
        { whitePieces.rook,   whitePieces.pawn, null, null, null, null, blackPieces.pawn, blackPieces.rook   },
        { whitePieces.knight, whitePieces.pawn, null, null, null, null, blackPieces.pawn, blackPieces.knight },
        { whitePieces.bishop, whitePieces.pawn, null, null, null, null, blackPieces.pawn, blackPieces.bishop },
        { whitePieces.queen,  whitePieces.pawn, null, null, null, null, blackPieces.pawn, blackPieces.queen  },
        { whitePieces.king,   whitePieces.pawn, null, null, null, null, blackPieces.pawn, blackPieces.king   },
        { whitePieces.bishop, whitePieces.pawn, null, null, null, null, blackPieces.pawn, blackPieces.bishop },
        { whitePieces.knight, whitePieces.pawn, null, null, null, null, blackPieces.pawn, blackPieces.knight },
        { whitePieces.rook,   whitePieces.pawn, null, null, null, null, blackPieces.pawn, blackPieces.rook   },
        };

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (prefabs[x, y] == null) continue;

                Piece piece = Instantiate(prefabs[x, y], piecesParent);
                Vector2Int cell = new Vector2Int(x, y);
                RectTransform rectTransform = piece.transform as RectTransform;
                SetUICoordsFromCell(cell, ref rectTransform);
                board[x, y] = piece;
            }
        }
    }
    public void ClearBoard()
    {
        // destroy any possible remaining pieces
        if (piecesParent.childCount > 0)
        {
            for (int i = 0; i < piecesParent.childCount; i++)
            {
                Destroy(piecesParent.GetChild(i).gameObject);
            }
        }
    }
    public void SetBoardStateFromString(string s)
    {
        ClearBoard();
        for (int i = 0; i < 64; i++)
        {
            int value = s[i] - '0';
            if (value == 0) continue;

            bool firstMove = value <= 12; // values 13+ mean pawn that has moved
            if (value > 12) value -= 12;

            Color color = value <= 6 ? Color.White : Color.Black;
            PieceType type = (PieceType)((value <= 6 ? value : value - 6) - 1);

            Piece piece = null;
            switch (type)
            {
                case PieceType.King:
                    piece = Instantiate(color == Color.White ? whitePieces.king : blackPieces.king, piecesParent);
                    break;
                case PieceType.Queen:
                    piece = Instantiate(color == Color.White ? whitePieces.queen : blackPieces.queen, piecesParent);
                    break;
                case PieceType.Bishop:
                    piece = Instantiate(color == Color.White ? whitePieces.bishop : blackPieces.bishop, piecesParent);
                    break;
                case PieceType.Knight:
                    piece = Instantiate(color == Color.White ? whitePieces.knight : blackPieces.knight, piecesParent);
                    break;
                case PieceType.Rook:
                    piece = Instantiate(color == Color.White ? whitePieces.rook : blackPieces.rook, piecesParent);
                    break;
                case PieceType.Pawn:
                    piece = Instantiate(color == Color.White ? whitePieces.pawn : blackPieces.pawn, piecesParent);
                    break;
            }

            piece.firstMove = firstMove;

            Vector2Int cell = new Vector2Int(i / 8, i % 8);
            RectTransform rectTransform = piece.transform as RectTransform;
            SetUICoordsFromCell(cell, ref rectTransform);
            board[cell.x, cell.y] = piece;
        }
    }
    public Piece ContainsPiece(Vector2Int cell)
    {
        return board[cell.x, cell.y];
    }


    public Color playerColor;

    [Header("Pieces")]
    public Pieces whitePieces;
    public Pieces blackPieces;
    public Piece selectedPiece;
    public void MoveSelectedPiece(Vector2Int target)
    {
        Vector2Int pos = GetCellFromUICoords(selectedPiece.transform as RectTransform);
        board[pos.x, pos.y] = null;

        if (board[target.x, target.y] != null)
            Destroy(board[target.x, target.y].gameObject);

        board[target.x, target.y] = selectedPiece;

        RectTransform rectTransform = selectedPiece.transform as RectTransform;
        SetUICoordsFromCell(target, ref rectTransform);

        if (selectedPiece.firstMove == true)
            selectedPiece.firstMove = false;

        NetworkManager.instance.SendOscMessage(new OscMessage("/player/move", selectedPiece.type.ToString(), selectedPiece.color.ToString(), $"{pos.x}{pos.y}", $"{target.x}{target.y}"));
    }

    public void HandlePlayerEvent(OscMessage msg, string[] addressParts)
    {
        switch (addressParts[1])
        {
            case "move":
                SetBoardStateFromString(msg[0] as string);
                break;
        }
    }

    [SerializeField] private RectTransform boardRectTransform;
    void SetUICoordsFromCell(Vector2Int cell, ref RectTransform rectTransform)
    {
        float cellSize = boardRectTransform.rect.width / 8f * boardRectTransform.lossyScale.x;
        Vector3 boardBottomLeft = boardRectTransform.position - new Vector3(
            boardRectTransform.pivot.x * boardRectTransform.rect.width * boardRectTransform.lossyScale.x,
            boardRectTransform.pivot.y * boardRectTransform.rect.height * boardRectTransform.lossyScale.y
        );
        rectTransform.position = boardBottomLeft + new Vector3(
            cellSize * cell.x + cellSize * 0.5f,
            cellSize * cell.y + cellSize * 0.5f
        );
    }
    public Vector2Int GetCellFromUICoords(RectTransform rectTransform)
    {
        float cellSize = boardRectTransform.rect.width / 8f * boardRectTransform.lossyScale.x;
        Vector2 boardBottomLeft = boardRectTransform.position - new Vector3(
            boardRectTransform.pivot.x * boardRectTransform.rect.width * boardRectTransform.lossyScale.x,
            boardRectTransform.pivot.y * boardRectTransform.rect.height * boardRectTransform.lossyScale.y
        );
        Vector2 pos = new Vector2(rectTransform.position.x, rectTransform.position.y);
        Vector2Int cell = Vector2Int.FloorToInt((pos - boardBottomLeft) / cellSize);
        return cell;
    }
    public bool IsValidCell(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < 8 && cell.y >= 0 && cell.y < 8;
    }

    [Header("Indicators")]
    [SerializeField] Transform indicatorParent;
    [SerializeField] GameObject movementIndicator;
    [SerializeField] GameObject captureIndicator;
    public void ShowIndicators(Piece piece)
    {
        Vector2Int pos = GetCellFromUICoords(piece.transform as RectTransform);

        foreach (Vector2Int cell in piece.strategy.CanMove(pos, piece))
        {
            GameObject indicator = Instantiate(movementIndicator, new Vector3(cell.x, cell.y), Quaternion.identity, indicatorParent);
            RectTransform rectTransform = indicator.transform as RectTransform;
            SetUICoordsFromCell(cell, ref rectTransform);
        }

        foreach (Vector2Int cell in piece.strategy.CanTake(pos, piece))
        {
            GameObject indicator = Instantiate(captureIndicator, new Vector3(cell.x, cell.y), Quaternion.identity, indicatorParent);
            RectTransform rectTransform = indicator.transform as RectTransform;
            SetUICoordsFromCell(cell, ref rectTransform);
        }
    }
    public void ClearIndicators()
    {
        for (int i = 0; i < indicatorParent.childCount; i++)
            Destroy(indicatorParent.GetChild(i).gameObject);
    }


    // currently unused
    [SerializeField] CanvasGroup waitTurnGroup;
    void EnableWaitTurnCover()
    {
        waitTurnGroup.alpha = 1;
        waitTurnGroup.interactable = true;
        waitTurnGroup.blocksRaycasts = true;
    }
    void DisableWaitTurnCover()
    {
        waitTurnGroup.alpha = 0;
        waitTurnGroup.interactable = false;
        waitTurnGroup.blocksRaycasts = false;
    }


    [SerializeField] CanvasGroup gameEndGroup;
    [SerializeField] TMP_Text gameEndText;
    void EnableGameEndCover(EndState endState)
    {
        gameEndGroup.alpha = 1;
        gameEndGroup.interactable = true;
        gameEndGroup.blocksRaycasts = true;

        gameEndText.text = endState switch { 
            EndState.Disconnected => "Opponent disconnected", 
            EndState.Black => "Black wins", 
            EndState.White => "White wins", 
            _ => "Unknown end state" 
        };
    }
    public void DisableGameEndCover()
    {
        gameEndGroup.alpha = 0;
        gameEndGroup.interactable = false;
        gameEndGroup.blocksRaycasts = false;
    }
}

[System.Serializable]
public class Pieces
{
    public Piece king;
    public Piece queen;
    public Piece bishop;
    public Piece knight;
    public Piece rook;
    public Piece pawn;
}

public enum EndState
{
    Disconnected,
    White,
    Black,
}