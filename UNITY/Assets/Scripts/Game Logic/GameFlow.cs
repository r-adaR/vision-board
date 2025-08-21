using TMPro;
using UnityEngine;
using static GameState;
using static Client;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class GameFlow : MonoBehaviour
{

    [SerializeField] private TMP_Text turn_text;
    [SerializeField] private TMP_Text x_score_text;
    [SerializeField] private TMP_Text o_score_text;
    [SerializeField] private Transform illegalMoveIndicator;

    [SerializeField] private Camera gameCam;
    private Color defaultCamColor;

    [SerializeField] private BoardVisuals board_visuals;

    [SerializeField] private Volume volume;
    private ChromaticAberration crAb;
    private PaniniProjection paPro;

    public static GameFlow flow_instance;

    public bool canScan = false; // blocks scanning at the start animation

    private Queue<Side[,]> board_scans_queue = new Queue<Side[,]>();
    private Dictionary<String, int> board_hash_map = new Dictionary<String, int>();

    private bool scanningBoard = false;

    private const int THRESHOLD = 7;
    private const int DICT_LENGTH = 10;

    private void Awake()
    {
        if (flow_instance != null) Destroy(gameObject);
        flow_instance = this;
        volume.profile.TryGet(out crAb);
        volume.profile.TryGet(out paPro);
    }

    public void StartGame()
    {
        turn_text.text = game_instance.currentPlayer == Side.X ? "It's X's turn!" : "It's O's turn!";
        int dummy;
        x_score_text.text = $"X SCORE: {game_instance.GetScore(Side.X, game_instance.board, true, out dummy)}";
        o_score_text.text = $"O SCORE: {game_instance.GetScore(Side.O, game_instance.board, true, out dummy)}";
        defaultCamColor = gameCam.backgroundColor;
        canScan = true;
    }


    //private void Update()
    //{

    //    if (!scanningBoard) // makes sure that  boardScanBuffers isn't called when one is already running
    //    {
    //        _ = boardScanBuffer();
    //        //_ = simpleBoardScanCaller();
    //    }
    //}

    float clock = 0;
    private void Update()
    {
        clock += Time.deltaTime;
        if (clock > 0.0f) // every second, see if the read board state is different

            if (!scanningBoard) // makes sure that  boardScanBuffers isn't called when one is already running
            {
                clock = 0;
                _ = boardScanBuffer();
                
            }
    }

    private async Task simpleBoardScanCaller()
    {
        scanningBoard = true;
        try
        {
            Side[,] newBoard = await Client.network_instance.GetBoardStateAsync();


            if (newBoard == null)
            {
                Debug.LogWarning("Board could not be read");
                scanningBoard = false;
                return;
            }
            else if (AreBoardsEqual(newBoard, game_instance.board))
                {
                    board_visuals.ClearErrors();
                    if (illegalMoveIndicator.localPosition.y > 0) // if illegal move indicator is shown on screen
                    {
                        illegalMoveIndicator.DOLocalMoveY(-50, 0.3f).SetEase(Ease.InExpo);
                    }
                    scanningBoard = false;
                    return;
                }
            else
            {
                boardConfirmationSteps(newBoard);
            }

        }
        finally
        {
            scanningBoard = false;
        }
    }

    private async Task boardScanBuffer()
    {
        // lock function
        scanningBoard = true;

        // get board from scanner
        Side[,] newBoard;
        try
        {
            // will wait here until the GetBoardStateAsync() function returns --> while waiting, will yield back to main thread (does not block)
            newBoard = await network_instance.GetBoardStateAsync();
        }
        catch (Exception)
        {
            scanningBoard = false;
            return;
        }

        if (newBoard == null)
        {
            Debug.LogWarning("Board could not be read");
            scanningBoard = false;
            return;
        }

        board_scans_queue.Enqueue(newBoard);
        if (!board_hash_map.ContainsKey(boardToString(newBoard)))
        {
            board_hash_map[boardToString(newBoard)] = 1;
        }
        else
        {
            board_hash_map[boardToString(newBoard)]++;
        }


        // check if less than 10 scans already
        if (board_scans_queue.Count <= DICT_LENGTH)
        {
            scanningBoard = false;
            return;
            // return?

        }
        else
        {
            // remove oldest board from queue
            Side[,] last_board = board_scans_queue.Dequeue();


            if (board_hash_map[boardToString(last_board)] > 1) // decrement count in hashmap
            {
                board_hash_map[boardToString(last_board)]--;
            }
            else // remove from hashmap if last one of that board is removed
            {
                board_hash_map.Remove(boardToString(last_board));
            }
        }

        // loop through dictionary/hashmap and find largest value
        int max_value = board_hash_map.Values.Max();
        String max_board = board_hash_map.FirstOrDefault(kvp => kvp.Value == max_value).Key;




        int Thresh = THRESHOLD;
        if (max_value < Thresh) // if the above or equal threshold, confirm board
        {
            // print($"many scans failed. Nulls: {1.0f * nulls / NUMBER_OF_SCANS > 0.2f}, Matched enough: {1.0f * correct / NUMBER_OF_SCANS < 0.7f}");

            print($"Threshold = {Thresh}, not met. Max same board =  {max_value} Board = {max_board}");
            scanningBoard = false;
            return;

        }

        Side[,] side_max_board = stringToBoard(max_board);
   

        if (AreBoardsEqual(side_max_board, game_instance.board))
        {
            board_visuals.ClearErrors();
            if (illegalMoveIndicator.localPosition.y > 0) // if illegal move indicator is shown on screen
            {
                illegalMoveIndicator.DOLocalMoveY(-50, 0.3f).SetEase(Ease.InExpo);
            }
            scanningBoard = false;
            return;
        }

        boardConfirmationSteps(side_max_board);



        scanningBoard = false;
    }

    private void boardConfirmationSteps(Side[,] side_max_board)
    {
        // board is confirmed to have made this move. "max_board" is the current board state.
        bool[,] errors = game_instance.GetErrors(game_instance.board, side_max_board, game_instance.currentPlayer);
        board_visuals.UpdateErrors(errors);

        if (game_instance.ThereAreErrors(errors))
        {
            // let the player know they messed up
            illegalMoveIndicator.DOLocalMoveY(50, 0.3f).SetEase(Ease.OutExpo);
        }
        else
        {
            // if illegal move indicator was shown on screen, bring it back down. at this point, all tiles are legal.
            if (illegalMoveIndicator.localPosition.y > 0) illegalMoveIndicator.DOLocalMoveY(-50, 0.3f).SetEase(Ease.InExpo);

            // advance the game!
            Tuple<int, int, Side> pieceAdded = game_instance.GetFirstNewPiece(side_max_board);
            game_instance.SetTile(pieceAdded.Item1, pieceAdded.Item2, pieceAdded.Item3);

            // IF WE GOT THE BONUS
            if (game_instance.bonusLoc != null && pieceAdded.Item1 == game_instance.bonusLoc.Item1 && pieceAdded.Item2 == game_instance.bonusLoc.Item2)
            {
                AudioPlayer.instance.PlaySound("bonus");
                board_visuals.EmitBonusParticles();
                board_visuals.HideBonus();
            }

            int old_x = game_instance.x_score;
            int old_o = game_instance.o_score;

            int fiveInARows;
            bool celebrateFiveInARow = false;

            game_instance.x_score = game_instance.GetScore(Side.X, side_max_board, true, out fiveInARows);
            if (game_instance.x_fiveInARows < fiveInARows)
            {
                game_instance.x_fiveInARows = fiveInARows; celebrateFiveInARow = true;
            }

            game_instance.o_score = game_instance.GetScore(Side.O, side_max_board, true, out fiveInARows);
            if (game_instance.o_fiveInARows < fiveInARows)
            {
                game_instance.o_fiveInARows = fiveInARows; celebrateFiveInARow = true;
            }


            // IF SOMEONE GETS SCORE, DO THIS:
            if (old_x < game_instance.x_score || old_o < game_instance.o_score)
            {
                int diff = Mathf.Max(game_instance.x_score - old_x, game_instance.o_score - old_o);
                if (crAb != null)
                {
                    crAb.intensity.value = 1f;
                    DOTween.To(() => crAb.intensity.value, (float v) => { crAb.intensity.value = v; }, 0f, 1f).SetEase(Ease.OutCubic);
                }
                if (paPro != null)
                {
                    paPro.distance.value = 0.2f;
                    DOTween.To(() => paPro.distance.value, (float v) => { paPro.distance.value = v; }, 0f, 1f).SetEase(Ease.OutCubic);
                }

                // ADD POINT GAINED INDICATOR ON TOP OF PIECE 
                board_visuals.SpawnPointsAbovePiece(pieceAdded.Item1, pieceAdded.Item2, diff);

                // change bg color based on score received
                if (diff >= 250)
                {
                    gameCam.backgroundColor = Color.cyan;
                    gameCam.DOColor(Color.yellow, 0.4f).OnComplete(() =>
                        gameCam.DOColor(Color.red, 0.4f).OnComplete(() => gameCam.DOColor(defaultCamColor, 0.4f))
                    );
                }
                else if (diff >= 150)
                {
                    gameCam.backgroundColor = Color.yellow;
                    gameCam.DOColor(Color.red, 0.4f).OnComplete(() => gameCam.DOColor(defaultCamColor, 0.4f));
                }
                else if (diff >= 100)
                {
                    gameCam.backgroundColor = Color.red;
                    gameCam.DOColor(defaultCamColor, 0.4f);
                }
                else
                {
                    gameCam.backgroundColor = Color.gray;
                    gameCam.DOColor(defaultCamColor, 0.4f);
                }
            }


            // IF SOMEBODY GOT A FIVE IN A ROW:
            if (celebrateFiveInARow)
            {
                AudioPlayer.instance.PlaySound("bell");
            }

            game_instance.AdvanceTurn(); // update backend to make it the next player's turn

            // UPDATE VISUALS
            board_visuals.SetPiece(pieceAdded.Item1, pieceAdded.Item2, pieceAdded.Item3);
            board_visuals.UpdateBonusLocation();

            // UPDATE TEXT
            turn_text.text = game_instance.currentPlayer == Side.X ? "It's X's turn!" : "It's O's turn!";
            x_score_text.text = $"X SCORE: {game_instance.x_score}";
            o_score_text.text = $"O SCORE: {game_instance.o_score}";
        }

        // if game over
        if (game_instance.gameOver)
        {
            // handle visual stuff here
            turn_text.text = "GAME OVER!";
            gameObject.SetActive(false);
        }
    }










    // DEBUG
    private string boardToString(Side[,] boardState)
    {
        if (boardState == null) return "NULL BOARD";

        // DEBUG
        string debugString = "";
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (boardState[y, x] == Side.NONE) debugString += "_";
                else debugString += boardState[y, x].ToString();
            }
        }

        return debugString;
    }

    private string boardToStringDebug(Side[,] boardState)
    {
        if (boardState == null) return "NULL BOARD";

        // DEBUG
        string debugString = "";
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (boardState[y, x] == Side.NONE) debugString += "_";
                else debugString += boardState[y, x].ToString();
                debugString += " ";
            }
            debugString += '\n';
        }

        return debugString;
    }


    private Side[,] stringToBoard(String data)
    {
        if (data == null) return new GameState.Side[5, 5];

        // DEBUG

        GameState.Side[,] boardState = new GameState.Side[5, 5];
        int count = 0;

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                count = (i * 5) + j;


                if (data[count] == 'X')
                {
                    boardState[i, j] = GameState.Side.X;
                }
                else if (data[count] == 'O')
                {
                    boardState[i, j] = GameState.Side.O;
                }
                else
                {
                    boardState[i, j] = GameState.Side.NONE;
                }
            }
        }

        return boardState;
    }

    private bool AreBoardsEqual(Side[,] _1, Side[,] _2)
    {
        if (_1 == null || _2 == null) return false;

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (_1[y, x] != _2[y, x]) return false;
            }
        }

        return true;
    }
}



