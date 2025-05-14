using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;
using static GameState;
using static Client;
using System.Collections;
using System;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class GameFlow : MonoBehaviour
{

    [SerializeField] private TMP_Text turn_text;
    [SerializeField] private TMP_Text x_score_text;
    [SerializeField] private TMP_Text o_score_text;

    [SerializeField] private BoardVisuals board_visuals;

    [SerializeField] private Volume volume;
    private ChromaticAberration crAb;
    private PaniniProjection paPro;

    public static GameFlow flow_instance;

    bool readingCoroutineActive = false;

    private const int NUMBER_OF_SCANS = 10;
    private const float DELAY_PER_SCAN = 0.01f;

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
        x_score_text.text = $"X SCORE: {game_instance.GetScore(Side.X, game_instance.board, true)}";
        o_score_text.text = $"O SCORE: {game_instance.GetScore(Side.O, game_instance.board, true)}";
    }

    private float clock = 0;
    private void Update()
    {
        if (!readingCoroutineActive) clock += Time.deltaTime; // only increase clock counter if we're currently not confirming the board state
        if (clock > 1) // every second, see if the read board state is different
        {
            clock = 0;
            Side[,] newBoard = network_instance.getBoardState();
            bool newBoardIsSame = AreBoardsEqual(newBoard, game_instance.board);
            if (newBoard != null && !newBoardIsSame)
            {
                // start the confirmation process
                StartCoroutine(ConfirmBoardState(newBoard));
            }
            else if (newBoard == null)
            {
                Debug.LogWarning("Board could not be read");
            }
            else
            {
                board_visuals.ClearErrors();
            }
        }
    }


    /// <summary>
    /// pass in a new board state as a parameter, this function will certify
    /// beyond a shadow of a doubt that the board is indeed this state, and make that move.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    private IEnumerator ConfirmBoardState(Side[,] _board)
    {
        readingCoroutineActive = true;

        // confirm board state
        int correct = 0;
        int nulls = 0;
        for (int i = 0; i < NUMBER_OF_SCANS; i++)
        {
            Side[,] newScan = network_instance.getBoardState();
            if (newScan == null) nulls++;
            else if (AreBoardsEqual(newScan, _board)) correct++;

            print($"nulls: {nulls}/{i}, correct: {correct}/{i}\nTARGET BOARD: \n{boardToString(_board)} \n\nSEEN BOARD: \n{boardToString(newScan)}");
            // DEBUG

            yield return new WaitForSeconds(DELAY_PER_SCAN);
        }

        // if 20% were nulls, or less than 80% of the scans matched,
        // ignore this new board state. the player is probably making a move
        if (1.0f * nulls /NUMBER_OF_SCANS > 0.2f || 1.0f * correct / NUMBER_OF_SCANS < 0.7f)
        {
            readingCoroutineActive = false;
            print($"many scans failed. Nulls: {1.0f*nulls / NUMBER_OF_SCANS > 0.2f}, Matched enough: {1.0f*correct / NUMBER_OF_SCANS < 0.7f}");
            yield break;
        }

        // board is confirmed to have made this move. "_board" is the current board state.
        bool[,] errors = game_instance.GetErrors(game_instance.board, _board, game_instance.currentPlayer);
        board_visuals.UpdateErrors(errors);

        if (game_instance.ThereAreErrors(errors))
        {
            // let the player know they messed up
            turn_text.text = "Please fix illegal move(s)";
        }
        else
        {
            // advance the game!
            Tuple<int, int, Side> pieceAdded = game_instance.GetFirstNewPiece(_board);
            game_instance.SetTile(pieceAdded.Item1, pieceAdded.Item2, pieceAdded.Item3);

            game_instance.AdvanceTurn(); // update backend to make it the next player's turn

            // UPDATE VISUALS
            board_visuals.SetPiece(pieceAdded.Item1, pieceAdded.Item2, pieceAdded.Item3);
            board_visuals.UpdateBonusLocation();

            // UPDATE TEXT
            turn_text.text = game_instance.currentPlayer == Side.X ? "It's X's turn!" : "It's O's turn!";

            int old_x = game_instance.x_score;
            int old_o = game_instance.o_score;
            game_instance.x_score = game_instance.GetScore(Side.X, _board, true);
            game_instance.o_score = game_instance.GetScore(Side.O, _board, true);

            // IF SOMEONE GETS SCORE, DO THIS:
            if (old_x < game_instance.x_score || old_o < game_instance.o_score)
            {
                if (crAb != null) { crAb.intensity.value = 1f; DOTween.To(() => crAb.intensity.value, (float v) => { crAb.intensity.value = v; }, 0f, 1f).SetEase(Ease.OutCubic); }
                if (paPro != null) { paPro.distance.value = 0.2f; DOTween.To(() => paPro.distance.value, (float v) => { paPro.distance.value = v; }, 0f, 1f).SetEase(Ease.OutCubic); }
            }

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

        readingCoroutineActive = false;
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
                debugString += " ";
            }
            debugString += '\n';
        }

        return debugString;
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

