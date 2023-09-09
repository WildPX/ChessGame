using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialMove
{
    None = 0,
    EnPassant = 1,
    Castling = 2,
    Promotion = 3
}

public class ChessBoard : MonoBehaviour
{
    public Sprite WhiteSquare;
    public Sprite BlackSquare;
    public Sprite ChooseTile;
    //public Sprite WhiteSquareSelected;
    //public Sprite BlackSquareSelected;
    public Sprite Highlighted;

    public GameObject[] WhiteChessPieces;
    public GameObject[] BlackChessPieces;
    public GameObject VictoryScreen;
    public GameObject VictoryText;

    // Game logic (Chessboard)
    // Logic counter to count which square to add right now: black or white. False = white, true = black
    private bool SquareColor;
    // Amount of tiles and their size
    private const int tilesX = 8, tilesY = 8;
    private const int size = 4;
    // Matrix for tiles
    private GameObject[,] tiles = new GameObject[tilesX, tilesY];
    private GameObject MovingTile;

    // Game logic (ChessPieces)
    // Matrix for chess pieces
    private ChessPiece[,] ChessPieces = new ChessPiece[tilesX, tilesY];
    // Dead pieces
    private List<ChessPiece> DeadWhite = new List<ChessPiece>();
    private List<ChessPiece> DeadBlack = new List<ChessPiece>();
    private Vector3 CurDeadWhitePiece = new Vector3(34, 27, -1);
    private Vector3 CurDeadBlackPiece = new Vector3(34, 10, -1);

    // Game logic (ChessPieces movement)
    private ChessPiece CurrentPiece;
    private Vector2Int CurrentTile;
    private List<Vector2Int> AvailableMoves;
    // for smooth movement
    private float startPosX, startPosY;
    // special movements
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    private SpecialMove specialMove;

    // Game logic (turns)
    // False = white, true = black
    private bool Turn;

    // Start is called before the first frame update
    private void Start()
    {
        GenerateAllTiles(size, tilesX, tilesY);
        SpawnAllPieces();
        PositionAllPieces();
        Turn = false;
    }

    private void Update()
    {
        //OperateSprites();
        RaycastHit2D rayHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (rayHit)
        {
            // Get the indexes of the tile i've hit
            Vector2Int hitPosition = TileIndex(rayHit.transform.gameObject);


            // If we're hovering a tile after not hovering any tiles
            if (CurrentTile == -Vector2Int.one)
            {
                CurrentTile = hitPosition;
                //tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Selected Tile");
            }

            // If we were already hovering a tile, change the previous one
            if (CurrentTile != hitPosition)
            {
                tiles[CurrentTile.x, CurrentTile.y].layer = LayerMask.NameToLayer("Tile");
                CurrentTile = hitPosition;
                //tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Selected Tile");
            }

            // If we press left mouse button
            if (Input.GetMouseButtonDown(0))
            {
                if (ChessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    // Is it our turn?
                    if ((ChessPieces[hitPosition.x, hitPosition.y].team == Team.White && Turn == false) ||
                        (ChessPieces[hitPosition.x, hitPosition.y].team == Team.Black && Turn == true))
                    {
                        CurrentPiece = ChessPieces[hitPosition.x, hitPosition.y];
                        // Get the list of available moves
                        AvailableMoves = CurrentPiece.GetAvailableMoves(ref ChessPieces, tilesX, tilesY);
                        // Get the list of special moves
                        specialMove = CurrentPiece.GetSpecialMoves(ref ChessPieces, ref moveList, ref AvailableMoves);

                        PreventCheck();
                        HighlightTiles();
                    }
                }
            }

            // On releasing click
            if (CurrentPiece && Input.GetMouseButtonUp(0))
            {
                Vector2Int PreviousPosition = new Vector2Int(CurrentPiece.currentX, CurrentPiece.currentY);

                bool ValidMove = MoveTo(CurrentPiece, hitPosition.x, hitPosition.y);
                if(!ValidMove)
                    CurrentPiece.transform.position = new Vector3(PreviousPosition.x * size, PreviousPosition.y * size, -1);
                
                CurrentPiece = null;
                RemoveHighlightTiles();
            }

            // Translating moving tile
            MovingTile.transform.position = new Vector3(hitPosition.x * size, hitPosition.y * size, -0.5f);
        }
        else
        {
            if (CurrentTile != -Vector2Int.one)
            {
                tiles[CurrentTile.x, CurrentTile.y].layer = LayerMask.NameToLayer("Tile");
                CurrentTile = -Vector2Int.one;
            }

            if (CurrentPiece && Input.GetMouseButtonUp(0))
            {
                CurrentPiece.transform.position = new Vector3(CurrentPiece.currentX * size, CurrentPiece.currentY * size, -1);
                CurrentPiece = null;
                RemoveHighlightTiles();
            }

            MovingTile.transform.position = new Vector3(-10, -10, -0.5f);
        }

        // Smooth movement
        if (CurrentPiece)
        {
            Vector3 mousePos;
            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            CurrentPiece.transform.localPosition = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY, -2);
        }
    }

    // For smooth movement
    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos;
            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            startPosX = mousePos.x - this.transform.localPosition.x;
            startPosY = mousePos.y - this.transform.localPosition.y;

        }
    }

    // Generate The Board
    private void GenerateAllTiles(float tileSize, int tileCountX, int tileCountY)
    {
        for (int i = 0; i < tileCountX; i++)
        {
            SquareColor = System.Convert.ToBoolean((i + 1) % 2);
            for (int j = 0; j < tileCountY; j++)
            {
                tiles[i, j] = GenerateSingleTile(tileSize, i, j);
                SquareColor = !SquareColor;
            }
        }

        // Generate moving tile
        MovingTile = new GameObject("Moving Tile");
        //MovingTile.transform.parent = transform;
        MovingTile.transform.position = new Vector3(-10, -10, -0.5f);
        MovingTile.AddComponent<SpriteRenderer>().sprite = ChooseTile;
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        // Creating a new tile
        GameObject tile = new GameObject(string.Format("{0}, {1}", x, y));
        tile.transform.parent = transform;
        tile.transform.position = new Vector3(x * tileSize, y * tileSize, 0);
        if (SquareColor)
            tile.AddComponent<SpriteRenderer>().sprite = BlackSquare;
        else
            tile.AddComponent<SpriteRenderer>().sprite = WhiteSquare;

        tile.AddComponent<Rigidbody2D>();
        tile.AddComponent<BoxCollider2D>();
        tile.GetComponent<Rigidbody2D>().isKinematic = true;
        tile.layer = LayerMask.NameToLayer("Tile");

        return tile;
    }

    // Generate The Chess Pieces
    private void SpawnAllPieces()
    {
        // White team
        ChessPieces[0, 0] = SpawnSinglePiece(Type.Rook, Team.White);
        ChessPieces[1, 0] = SpawnSinglePiece(Type.Knight, Team.White);
        ChessPieces[2, 0] = SpawnSinglePiece(Type.Bishop, Team.White);
        ChessPieces[3, 0] = SpawnSinglePiece(Type.Queen, Team.White);
        ChessPieces[4, 0] = SpawnSinglePiece(Type.King, Team.White);
        ChessPieces[5, 0] = SpawnSinglePiece(Type.Bishop, Team.White);
        ChessPieces[6, 0] = SpawnSinglePiece(Type.Knight, Team.White);
        ChessPieces[7, 0] = SpawnSinglePiece(Type.Rook, Team.White);
        for (int i = 0; i < tilesX; i++)
            ChessPieces[i, 1] = SpawnSinglePiece(Type.Pawn, Team.White);

        // Black team
        ChessPieces[0, 7] = SpawnSinglePiece(Type.Rook, Team.Black);
        ChessPieces[1, 7] = SpawnSinglePiece(Type.Knight, Team.Black);
        ChessPieces[2, 7] = SpawnSinglePiece(Type.Bishop, Team.Black);
        ChessPieces[3, 7] = SpawnSinglePiece(Type.Queen, Team.Black);
        ChessPieces[4, 7] = SpawnSinglePiece(Type.King, Team.Black);
        ChessPieces[5, 7] = SpawnSinglePiece(Type.Bishop, Team.Black);
        ChessPieces[6, 7] = SpawnSinglePiece(Type.Knight, Team.Black);
        ChessPieces[7, 7] = SpawnSinglePiece(Type.Rook, Team.Black);
        for (int i = 0; i < tilesX; i++)
            ChessPieces[i, 6] = SpawnSinglePiece(Type.Pawn, Team.Black);
    }
    private ChessPiece SpawnSinglePiece(Type type, Team team)
    {
        ChessPiece tmp;
        if (team == Team.White)
            tmp = Instantiate(WhiteChessPieces[(int)type - 1], transform).GetComponent<ChessPiece>();
        else
            tmp = Instantiate(BlackChessPieces[(int)type - 1], transform).GetComponent<ChessPiece>();

        tmp.type = type;
        tmp.team = team;

        return tmp;
    }

    // Positioning of chess pieces
    private void PositionAllPieces()
    {
        for (int i = 0; i < tilesX; i++)
            for (int j = 0; j < tilesY; j++)
                if (ChessPieces[i, j] != null)
                    PositionSinglePiece(i, j, size);
    }

    private void PositionSinglePiece(int x, int y, int tileSize)
    {
        ChessPieces[x, y].currentX = x;
        ChessPieces[x, y].currentY = y;
        ChessPieces[x, y].transform.position = new Vector3(x * tileSize, y * tileSize, -1);
    }

    // Helper functions
    // Get the chess piece position
    private Vector2Int TileIndex(GameObject hitInfo)
    {
        for (int i = 0; i < tilesX; i++)
            for (int j = 0; j < tilesY; j++)
                if (tiles[i, j] == hitInfo)
                    return new Vector2Int(i, j);

        return -Vector2Int.one;
    }

    //private Vector2Int ChessPieceIndex(GameObject hitInfo)
    //{
    //    for (int i = 0; i < tilesX; i++)
    //        for (int j = 0; j < tilesY; j++)
    //            if (ChessPieces[i, j] == hitInfo)
    //                return new Vector2Int(i, j);

    //    return -Vector2Int.one;
    //}

    // Sprite things
    private void HighlightTiles()
    {
        for (int i = 0; i < AvailableMoves.Count; i++)
        {
            tiles[AvailableMoves[i].x, AvailableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
            tiles[AvailableMoves[i].x, AvailableMoves[i].y].GetComponent<SpriteRenderer>().sprite = Highlighted;
            //tiles[AvailableMoves[i].x, AvailableMoves[i].y].GetComponent<SpriteRenderer>().color = new Color (1f, 1f, 1f, 0.8f);
        }
    }

    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < AvailableMoves.Count; i++)
        {
            tiles[AvailableMoves[i].x, AvailableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
            if ((AvailableMoves[i].x + AvailableMoves[i].y) % 2 == 0)
                tiles[AvailableMoves[i].x, AvailableMoves[i].y].GetComponent<SpriteRenderer>().sprite = BlackSquare;
            else
                tiles[AvailableMoves[i].x, AvailableMoves[i].y].GetComponent<SpriteRenderer>().sprite = WhiteSquare;
            //tiles[AvailableMoves[i].x, AvailableMoves[i].y].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }

        AvailableMoves.Clear();
    }

    private bool MoveTo(ChessPiece curPiece, int x, int y)
    {
        if (!ContainsValidMove(ref AvailableMoves, new Vector2(x, y)))
            return false;


        Vector2Int previousPosition = new Vector2Int(curPiece.currentX, curPiece.currentY);

        // Is there another piece on the target?
        if (ChessPieces[x, y] != null)
        {
            ChessPiece ocp = ChessPieces[x, y];
            if (curPiece.team == ocp.team)
                return false;

            // Dead scenario
            if (ocp.team == 0)
            {
                //// Checkmate
                //if (ocp.type == Type.King)
                //    CheckMate(1);
                DeadWhite.Add(ocp);
                ocp.transform.position = CurDeadWhitePiece;
                ocp.transform.localScale = new Vector3(0.7f, 0.7f, 1);
                AddOneToTheCurWhiteDead();
            }
            else
            {
                //if (ocp.type == Type.King)
                //    CheckMate(0);
                //DeadBlack.Add(ocp);
                ocp.transform.position = CurDeadBlackPiece;
                ocp.transform.localScale = new Vector3(0.7f, 0.7f, 1);
                AddOneToTheCurBlackDead();
            }
        }

        // Translate pieces in Vector
        ChessPieces[x, y] = curPiece;
        ChessPieces[previousPosition.x, previousPosition.y] = null;

        PositionSinglePiece(x, y, size);

        Turn = !Turn;
        moveList.Add(new Vector2Int[] { previousPosition, new Vector2Int(x, y) });
        // Check for special move
        ProcessSpecialMove();

        // Check for checkmate
        if (CheckForCheckmate())
        {
            CheckMate(ConvertTeamToInt(CurrentPiece.team));
        }

        return true;
    }

    // Operate dead chesspieces
    private void AddOneToTheCurWhiteDead()
    {
        if (CurDeadWhitePiece.x == 40)
        {
            CurDeadWhitePiece.y -= 3;
            CurDeadWhitePiece.x = 34;
        }
        else
            CurDeadWhitePiece.x += 2;
    }
    private void AddOneToTheCurBlackDead()
    {
        if (CurDeadBlackPiece.x == 40)
        {
            CurDeadBlackPiece.y -= 3;
            CurDeadBlackPiece.x = 34;
        }
        else
            CurDeadBlackPiece.x += 2;
    }

    // if list contains a valid move -> true
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;

        return false;
    }


    private void PreventCheck()
    {
        ChessPiece targetKing = null;
        
        for (int i = 0; i < tilesX; i++)
            for (int j = 0; j < tilesY; j++)
                if (ChessPieces[i, j] != null)
                    if (ChessPieces[i, j].type == Type.King)
                        if (ChessPieces[i, j].team == CurrentPiece.team)
                            targetKing = ChessPieces[i, j];

        // We'll delete moves that are putting king in check (why we're sending 'ref')
        SimulateMoveForSinglePiece(CurrentPiece, ref AvailableMoves, targetKing);
    }

    private void SimulateMoveForSinglePiece(ChessPiece piece, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        // Save current values to reset after the call
        int curX = piece.currentX, curY = piece.currentY;
        List<Vector2Int> MovesToRemove = new List<Vector2Int>();

        // Going through all the moves, simulate them and see if we're in check
        for (int i = 0; i < moves.Count; i++)
        {
            // Current coordinates to simulate moves
            int simX = moves[i].x, simY = moves[i].y;

            Vector2Int kingPositionThisSimulation = new Vector2Int(targetKing.currentX, targetKing.currentY);
            // Did we simulate the king's move?
            if (piece.type == Type.King)
                kingPositionThisSimulation = new Vector2Int(simX, simY);

            // Copy of [,], not ref
            ChessPiece[,] Simulation = new ChessPiece[tilesX, tilesY];
            List<ChessPiece> SimulationAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < tilesX; x++)
            {
                for (int y = 0; y < tilesY; y++)
                {
                    if (ChessPieces[x, y] != null)
                    {
                        Simulation[x, y] = ChessPieces[x, y];
                        if (Simulation[x, y].team != piece.team)
                            SimulationAttackingPieces.Add(Simulation[x, y]);
                    }
                }
            }

            // Simulate move
            Simulation[curX, curY] = null;
            piece.currentX = simX;
            piece.currentY = simY;
            Simulation[simX, simY] = piece;

            // Did one of the pieces got taken down during simulation?
            var DeadPiece = SimulationAttackingPieces.Find(x => x.currentX == simX && x.currentY == simY);
            if (DeadPiece != null)
                SimulationAttackingPieces.Remove(DeadPiece);

            // Get all simulated attacking pieces moves
            List<Vector2Int> SimulationMoves = new List<Vector2Int>();
            for (int a = 0; a < SimulationAttackingPieces.Count; a++)
            {
                var PieceMoves = SimulationAttackingPieces[a].GetAvailableMoves(ref Simulation, tilesX, tilesY);
                for (int b = 0; b < PieceMoves.Count; b++)
                {
                    SimulationMoves.Add(PieceMoves[b]);
                }
            }

            // Is the king in trouble ? Remove the move : Don't
            if (ContainsValidMove(ref SimulationMoves, kingPositionThisSimulation))
            {
                MovesToRemove.Add(moves[i]);
            }

            // Restore start piece data
            piece.currentX = curX;
            piece.currentY = curY;
        }

        // Remove illegal moves
        for (int i = 0; i < MovesToRemove.Count; i++)
        {
            moves.Remove(MovesToRemove[i]);
        }
    }

    // Not done
    //private void SimulateMoveForSinglePieceStalemate(ChessPiece piece, ref List<Vector2Int> moves, ChessPiece targetKing)
    //{
    //    // Save current values to reset after the call
    //    int curX = piece.currentX, curY = piece.currentY;
    //    List<Vector2Int> MovesToRemove = new List<Vector2Int>();

    //    // Going through all the moves, simulate them and see if we're in check
    //    for (int i = 0; i < moves.Count; i++)
    //    {
    //        // Current coordinates to simulate moves
    //        int simX = moves[i].x, simY = moves[i].y;

    //        Vector2Int kingPositionThisSimulation = new Vector2Int(targetKing.currentX, targetKing.currentY);
    //        // Did we simulate the king's move?
    //        if (piece.type == Type.King)
    //            kingPositionThisSimulation = new Vector2Int(simX, simY);

    //        // Copy of [,], not ref
    //        ChessPiece[,] Simulation = new ChessPiece[tilesX, tilesY];
    //        List<ChessPiece> SimulationAttackingPieces = new List<ChessPiece>();
    //        for (int x = 0; x < tilesX; x++)
    //        {
    //            for (int y = 0; y < tilesY; y++)
    //            {
    //                if (ChessPieces[x, y] != null)
    //                {
    //                    Simulation[x, y] = ChessPieces[x, y];
    //                    if (Simulation[x, y].team != piece.team)
    //                        SimulationAttackingPieces.Add(Simulation[x, y]);
    //                }
    //            }
    //        }

    //        // Simulate move
    //        Simulation[curX, curY] = null;
    //        piece.currentX = simX;
    //        piece.currentY = simY;
    //        Simulation[simX, simY] = piece;

    //        // Did one of the pieces got taken down during simulation?
    //        var DeadPiece = SimulationAttackingPieces.Find(x => x.currentX == simX && x.currentY == simY);
    //        if (DeadPiece != null)
    //            SimulationAttackingPieces.Remove(DeadPiece);

    //        // Get all simulated attacking pieces moves
    //        List<Vector2Int> SimulationMoves = new List<Vector2Int>();
    //        for (int a = 0; a < SimulationAttackingPieces.Count; a++)
    //        {
    //            var PieceMoves = SimulationAttackingPieces[a].GetAvailableMoves(ref Simulation, tilesX, tilesY);
    //            for (int b = 0; b < PieceMoves.Count; b++)
    //            {
    //                SimulationMoves.Add(PieceMoves[b]);
    //            }
    //        }

    //        // Is the king in trouble ? Remove the move : Don't
    //        if (!ContainsValidMove(ref SimulationMoves, kingPositionThisSimulation))
    //        {
    //            MovesToRemove.Add(moves[i]);
    //        }

    //        // Restore start piece data
    //        piece.currentX = curX;
    //        piece.currentY = curY;
    //    }

    //    // Remove illegal moves
    //    for (int i = 0; i < MovesToRemove.Count; i++)
    //    {
    //        moves.Remove(MovesToRemove[i]);
    //    }
    //}

    private bool CheckForCheckmate()
    {
        // Which team played last
        var LastMove = moveList[moveList.Count - 1];
        int targetTeam = (ChessPieces[LastMove[1].x, LastMove[1].y].team == 0) ? 1 : 0;

        List<ChessPiece> AttackingPieces = new List<ChessPiece>();
        List<ChessPiece> DefendingPieces = new List<ChessPiece>();

        ChessPiece targetKing = null;
        for (int i = 0; i < tilesX; i++)
            for (int j = 0; j < tilesY; j++)
                if (ChessPieces[i, j] != null)
                {
                    if (ChessPieces[i, j].team == ConvertIntToTeam(targetTeam))
                    {
                        DefendingPieces.Add(ChessPieces[i, j]);
                        if (ChessPieces[i, j].type == Type.King)
                            targetKing = ChessPieces[i, j];
                    }
                    else
                    {
                        AttackingPieces.Add(ChessPieces[i, j]);
                    }
                }

        
        // Is the king attacked right now?
        List<Vector2Int> CurrentAvailableMoves = new List<Vector2Int>();
        for (int i = 0; i < AttackingPieces.Count; i++)
        {
            var PieceMoves = AttackingPieces[i].GetAvailableMoves(ref ChessPieces, tilesX, tilesY);
            for (int j = 0; j < PieceMoves.Count; j++)
                CurrentAvailableMoves.Add(PieceMoves[j]);
        }

        // Is king in check?
        if (ContainsValidMove(ref CurrentAvailableMoves, new Vector2Int (targetKing.currentX, targetKing.currentY))) 
        {
            // King is under attack. Can we move something to help him?
            for (int i = 0; i < DefendingPieces.Count; i++)
            {
                List<Vector2Int> DefendingMoves = DefendingPieces[i].GetAvailableMoves(ref ChessPieces, tilesX, tilesY);
                // Will remove the move we're not able to do
                SimulateMoveForSinglePiece(DefendingPieces[i], ref DefendingMoves, targetKing);

                if (DefendingMoves.Count != 0)
                    return false;
            }

            // Checkmate
            return true;
        }

        return false;
    }

    private void CheckMate(int winningTeam)
    {
        Victory(winningTeam);
    }

    private void Victory(int winningTeam)
    {
        VictoryScreen.SetActive(true);
        VictoryText.SetActive(true);
        VictoryText.transform.GetChild(winningTeam).gameObject.SetActive(true);
    }
    public void OnResetButton()
    {
        VictoryScreen.SetActive(false);
        VictoryText.SetActive(false);
        VictoryText.transform.GetChild(0).gameObject.SetActive(false);
        VictoryText.transform.GetChild(1).gameObject.SetActive(false);

        // Clean up
        for (int i = 0; i < tilesX; i++)
            for (int j = 0; j < tilesY; j++)
            {
                if (ChessPieces[i, j] != null)
                    Destroy(ChessPieces[i, j].gameObject);

                ChessPieces[i, j] = null;
            }

        for (int i = 0; i < DeadWhite.Count; i++)
            Destroy(DeadWhite[i].gameObject);
        for (int i = 0; i < DeadBlack.Count; i++)
            Destroy(DeadBlack[i].gameObject);

        DeadWhite.Clear();
        DeadBlack.Clear();
        moveList.Clear();
        AvailableMoves.Clear();

        CurrentPiece = null;

        for (int i = 0; i < tilesX; i++)
        {
            SquareColor = System.Convert.ToBoolean((i + 1) % 2);
            for (int j = 0; j < tilesY; j++)
            {
                tiles[i, j].layer = LayerMask.NameToLayer("Tile");
                if (SquareColor)
                    tiles[i, j].GetComponent<SpriteRenderer>().sprite = BlackSquare;
                else
                    tiles[i, j].GetComponent<SpriteRenderer>().sprite = WhiteSquare;
                SquareColor = !SquareColor;
            }
        }



        SpawnAllPieces();
        PositionAllPieces();
        Turn = false;
    }
    public void OnExitButton()
    {
        Application.Quit();  
    }

    // Special moves
    private void ProcessSpecialMove()
    {
        if (specialMove == SpecialMove.EnPassant)
        {
            Vector2Int[] newMove = moveList[moveList.Count - 1];
            Vector2Int[] targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece currentPawn = ChessPieces[newMove[1].x, newMove[1].y];
            ChessPiece enemyPawn = ChessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if (currentPawn.currentX == enemyPawn.currentX)
            {
                if (currentPawn.currentY == enemyPawn.currentY - 1 || currentPawn.currentY == enemyPawn.currentY + 1)
                {
                    if (enemyPawn.team == Team.White)
                    {
                        DeadWhite.Add(enemyPawn);
                        enemyPawn.transform.position = CurDeadWhitePiece;
                        enemyPawn.transform.localScale = new Vector3(0.7f, 0.7f, 1);
                        AddOneToTheCurWhiteDead();
                    }
                    else
                    {
                        DeadBlack.Add(enemyPawn);
                        enemyPawn.transform.position = CurDeadBlackPiece;
                        enemyPawn.transform.localScale = new Vector3(0.7f, 0.7f, 1);
                        AddOneToTheCurBlackDead();
                    }
                    ChessPieces[enemyPawn.currentX, enemyPawn.currentY] = null;
                }
            }
        }

        if (specialMove == SpecialMove.Promotion)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = ChessPieces[lastMove[1].x, lastMove[1].y];


            if (targetPawn.type == Type.Pawn)
            {
                if (targetPawn.team == Team.White && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(Type.Queen, Team.White);
                    Destroy(ChessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    ChessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y, size);
                }
                if (targetPawn.team == Team.Black && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(Type.Queen, Team.Black);
                    Destroy(ChessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    ChessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y, size);
                }
            }
        }

        if (specialMove == SpecialMove.Castling)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            
            // Left rook
            if (lastMove[1].x == 2)
            {
                // King has already moved and now we're moving the rook
                if (lastMove[1].y == 0) // Whites
                {
                    ChessPiece rook = ChessPieces[0, 0];
                    ChessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0, size);
                    ChessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7) // Blacks
                {
                    ChessPiece rook = ChessPieces[0, 7];
                    ChessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7, size);
                    ChessPieces[0, 7] = null;
                }
            }
            // Right rook
            else if (lastMove[1].x == 6)
            {
                // King has already moved and now we're moving the rook
                if (lastMove[1].y == 0) // Whites
                {
                    ChessPiece rook = ChessPieces[7, 0];
                    ChessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0, size);
                    ChessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7) // Blacks
                {
                    ChessPiece rook = ChessPieces[7, 7];
                    ChessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7, size);
                    ChessPieces[7, 7] = null;
                }
            }
        }
    }

    private int ConvertTeamToInt(Team t)
    {
        if (t == Team.White)
            return 0;
        else
            return 1;
    }

    private Team ConvertIntToTeam(int x)
    {
        if (x == 0)
            return Team.White;
        else
            return Team.Black;
    }
}