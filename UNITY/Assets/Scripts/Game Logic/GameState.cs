using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    
    public enum Side
    {
        NONE,
        X,
        O,
    }

    public bool[,] errors { get; private set; } // board that highlights if any squares are in violation of illegal moves
    public Side[,] board {  get; private set; } // board w/ x's and o's


    public Tuple<int, int> bonusLoc { get; private set; }
    public Side currentPlayer { get; private set; }

    public int turn_number { get; private set; } = 1;
    public int x_score { get; private set; } = 0;
    public int x_bonuses { get; private set; } = 0; // ex: if x_bonuses == 1 --> player x should get 50 extra points
    public int o_score { get; private set; } = 0;
    public int o_bonuses { get; private set; } = 0; // ex: if o_bonuses == 2 --> player o should get 100 extra points


    public bool gameOver { get; private set; } = false;


    public static GameState game_instance { get; private set; }

    private void Awake()
    {
        if (game_instance == null) game_instance = this;
        else Destroy(gameObject);

        board = new Side[5, 5];
        errors = new bool[5, 5];
        bonusLoc = GetRandomEmptySquare(this.board);

        currentPlayer = Side.X;

        
    }

    private void Start()
    {
        GameFlow.flow_instance.StartGame();
    }


    // GAME STATE UPDATE FUNCTIONS //
    // These functions are not really as unit-testable on their own like the helper functions because they rely on the objects in this class

    /// <summary>
    /// Sets a tile in the 2D array "board" at coordinates y, x with side "side"
    /// </summary>
    /// <param name="y"></param>
    /// <param name="x"></param>
    /// <param name="side"></param>
    public void SetTile(int y, int x, Side side)
    {
        if (board[y,x] != Side.NONE)
        {
            Debug.LogError("You are setting a piece onto a non-empty tile. This should NOT happen.");
            return;
        }

        board[y, x] = side;
        if (bonusLoc.Item1 == y && bonusLoc.Item2 == x)
        {
            if (side == Side.X) x_bonuses++;
            else if (side == Side.O) o_bonuses++;
        }
    }


    /// <summary>
    /// returns the coordinates of the first new piece and the side at those coordinates as (item1 = y, item2 = x, item3 = side)
    /// </summary>
    /// <param name="newBoard"></param>
    /// <returns></returns>
    public Tuple<int, int, Side> GetFirstNewPiece(Side[,] newBoard)
    {
        if (ThereAreErrors(GetErrors(this.board, newBoard, currentPlayer)))
        {
            Debug.LogWarning("Get first new piece result is unpredictable. Errors were detected in new board state. Returning null.");
            return null;
        }
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
                if (newBoard[y, x] != this.board[y, x]) return new Tuple<int, int, Side>(y, x, newBoard[y, x]);

        Debug.LogWarning("No new pieces detected when using GetFirstNewPiece. Returning null.");
        return null;
    }

    /// <summary>
    /// increment the turn counter, change the current player, and find a new spot for the bonus. If there are no new moves: declares the game is over.
    /// </summary>
    public void AdvanceTurn()
    {
        if (turn_number == 25)
        {
            gameOver = true; // the visual side of things will handle what to do from here
        }
        else
        {
            turn_number++;
            bonusLoc = GetRandomEmptySquare(this.board);
            if (currentPlayer == Side.X) currentPlayer = Side.O;
            else currentPlayer = Side.X;
        }
    }



    // HELPER FUNCTIONS //


    /// <summary>
    /// gets a random empty square on the board. RETURNS NULL if there are no empty squares
    /// </summary>
    /// <returns></returns>
    public Tuple<int, int> GetRandomEmptySquare(Side[,] board)
    {
        List<Tuple<int, int>> l = new List<Tuple<int, int>>();
        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
                if (board[y, x] == Side.NONE)
                    l.Add(new Tuple<int, int>(y, x));

        if (l.Count == 0) return null;

        return l[UnityEngine.Random.Range(0, l.Count)];
    }


    /// <summary>
    /// returns a multidimensional array of booleans showing if any squares on 
    /// the new board state would be the result of an illegal move (replacing squares, wrong player making a move, etc.)
    /// </summary>
    /// <param name="board"></param>
    /// <param name="newBoard"></param>
    /// <param name="currentPlayer"></param>
    /// <returns></returns>
    public bool[,] GetErrors(Side[,] board, Side[,] newBoard, Side currentPlayer)
    {

        bool[,] errorArray = new bool[5, 5];

        if (currentPlayer == Side.NONE)
        {
            Debug.LogError("currentPlayer passed in was NONE!");
            return errorArray;
        }

        int newPieces = 0;
        Tuple<int, int> firstNewPiece = null;

        for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                // square had something on it and now it's different
                if (board[y,x] != Side.NONE && newBoard[y,x] != board[y, x])
                {
                    errorArray[y,x] = true;
                }
                // square had nothing on it but it's now the not-current player's piece
                else if (board[y,x] == Side.NONE && (newBoard[y, x] != Side.NONE || newBoard[y,x] != currentPlayer))
                {
                    errorArray[y,x] = true;
                }
                else if (board[y, x] == Side.NONE && newBoard[y, x] == currentPlayer) // square had nothing on it but it's now the new player
                {
                    errorArray[y, x] = newPieces > 0; // it's an error if there were already new pieces placed down
                    newPieces++;
                }
            }

        if (firstNewPiece != null && newPieces > 1) // if there's more than 1 new piece, that means the first one is an error as well 
        {
            errorArray[firstNewPiece.Item1, firstNewPiece.Item2] = true;
        }

        return errorArray;
    }


    /// <summary>
    /// checks to see if there are any errors saved into the errors array in GameState
    /// </summary>
    /// <returns></returns>
    public bool ThereAreErrors(bool[,] errorArray)
    {
        foreach (bool item in errorArray)
        {
            if (item) return true;
        }
        return false;
    }


    /// <summary>
    /// returns a player's score from a side of a given board
    /// </summary>
    public int GetScore(Side side, Side[,] board, bool includeBonuses)
    {
        if (side == Side.NONE)
        {
            Debug.LogError("side passed in was NONE!");
            return -1;
        }

        // we only care about updating the score for the one side we passed in
        int newScore = 0;

        // check for 3/4/5 in a rows in each row, column, and diagonal separately, then add up all the points
        
        // ----------------------------- //

        // each row:
        for (int y=0; y<5; y++) // go through each row (each y coordinate is its own row)
        {
            // keep track of how many pieces the loop has seen in a row that = the side parameter.
            // Ex: side == Side.X --> [X X X O O] will see currCount = 1, 2, and then 3, before breaking out
            int currCount = 0;
            for (int x=0; x<5; x++)
            {
                if (board[y, x] == side) // if the current board coordinate's side is the same as the side we passed into this function
                {
                    currCount++;
                }
                else
                {
                    if (currCount >= 3)
                    {
                        newScore += 100 + (100 * (currCount - 3)); // 100 points for 3, 200 points for 4, and 300 points for 5 in a row

                        // because the dimensions are 5x5, if we see anything 3 or more in a row (currCount >= 3),
                        // we will not find anything else in that row worth tallying to the score, so we can continue to the next row early
                        continue;
                    } 
                    else
                    {
                        currCount = 0;
                    }
                }
            }
            // if we hit the end of the loop and still counting up desirable pieces in a row, add any points we would've gotten.
            // Ex: side == Side.X --> [O X X X X] will hit the end of this loop with currCount = 4
            if (currCount >= 3) newScore += 100 + (100 * (currCount - 3));
        }

        // each column:
        for (int x = 0; x < 5; x++)
        {
            // exact same as the row stuff above, but x and y swapped. check above comments for explanation on logic.
            int currCount = 0;
            for (int y = 0; y < 5; y++)
            {
                if (board[y, x] == side)
                {
                    currCount++;
                }
                else
                {
                    if (currCount >= 3)
                    {
                        newScore += 100 + (100 * (currCount - 3));
                        continue;
                    }
                    else
                    {
                        currCount = 0;
                    }
                }
            }
            if (currCount >= 3) newScore += 100 + (100 * (currCount - 3));
        }

        // each topLeft->bottomRight diagonal
        for (int i=2; i>=0; i--)
        {
            int currCount = 0;
            int x = 0;
            for (int y = i; x < 5 && y < 5; x++, y++)
            {
                if (board[y, x] == side)
                {
                    currCount++;
                }
                else
                {
                    if (currCount >= 3)
                    {
                        newScore += 100 + (100 * (currCount - 3));
                        continue;
                    }
                    else
                    {
                        currCount = 0;
                    }
                }
            }
            if (currCount >= 3) newScore += 100 + (100 * (currCount - 3));
        }
        for (int i = 2; i > 0; i--)
        {
            int currCount = 0;
            int y = 0;
            for (int x = i; x < 5 && y < 5; x++, y++)
            {
                if (board[y, x] == side)
                {
                    currCount++;
                }
                else
                {
                    if (currCount >= 3)
                    {
                        newScore += 100 + (100 * (currCount - 3));
                        continue;
                    }
                    else
                    {
                        currCount = 0;
                    }
                }
            }
            if (currCount >= 3) newScore += 100 + (100 * (currCount - 3));
        }

        // each topRight->bottomLeft diagonal
        for (int i = 2; i < 5; i++)
        {
            int currCount = 0;
            int x = 0;
            for (int y = i; x < 5 && y > 0; x++, y--)
            {
                if (board[y, x] == side)
                {
                    currCount++;
                }
                else
                {
                    if (currCount >= 3)
                    {
                        newScore += 100 + (100 * (currCount - 3));
                        continue;
                    }
                    else
                    {
                        currCount = 0;
                    }
                }
            }
            if (currCount >= 3) newScore += 100 + (100 * (currCount - 3));
        }
        for (int i = 2; i > 0; i--)
        {
            int currCount = 0;
            int y = 4;
            for (int x = i; x < 5 && y > 0; x++, y--)
            {
                if (board[y, x] == side)
                {
                    currCount++;
                }
                else
                {
                    if (currCount >= 3)
                    {
                        newScore += 100 + (100 * (currCount - 3));
                        continue;
                    }
                    else
                    {
                        currCount = 0;
                    }
                }
            }
            if (currCount >= 3) newScore += 100 + (100 * (currCount - 3));
        }


        if (side == Side.X) return newScore + (includeBonuses ? x_bonuses * 50 : 0);
        else return newScore + (includeBonuses ? o_bonuses * 50 : 0);
    }

}
