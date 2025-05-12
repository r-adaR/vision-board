using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;
using static GameState;
using static Client;
using System.Collections;
using System;

public class GameFlow : MonoBehaviour
{

    [SerializeField] private TMP_Text turn_text;
    [SerializeField] private TMP_Text x_score_text;
    [SerializeField] private TMP_Text o_score_text;

    [SerializeField] private BoardVisuals board_visuals;

    public static GameFlow flow_instance;

    bool readingCoroutineActive = false;

    private const int NUMBER_OF_SCANS = 100;
    private const float DELAY_PER_SCAN = 0.02f;

    private void Awake()
    {
        if (flow_instance != null) Destroy(gameObject);
        flow_instance = this;
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
            if (newBoard != null && newBoard != game_instance.board)
            {
                // start the confirmation process
                StartCoroutine(ConfirmBoardState(newBoard));
            }
            else if (newBoard == null)
            {
                Debug.LogWarning("Board could not be read");
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
            else if (newScan == _board) correct++;
            yield return new WaitForSeconds(DELAY_PER_SCAN);
        }

        // if 20% were nulls, or less than 80% of the scans matched,
        // ignore this new board state. the player is probably making a move
        if (nulls/NUMBER_OF_SCANS > 0.2 || correct / NUMBER_OF_SCANS < 0.8)
        {
            readingCoroutineActive = false;
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
            x_score_text.text = $"X SCORE: {game_instance.GetScore(Side.X, _board, true)}";
            o_score_text.text = $"O SCORE: {game_instance.GetScore(Side.O, _board, true)}";
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

}
