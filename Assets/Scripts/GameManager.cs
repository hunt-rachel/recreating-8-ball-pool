using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    //instance of class
    public static GameManager Instance;
    
    public GameState gameState;

    public static event Action<GameState> OnGameStateChanged;

    //keeps track of each player turns in case turns are awarded after a foul
    public int P1NumOfTurns = 1;
    public int P2NumOfTurns = 0;
    
    //timer for each player turn
    public float timeLeft;

    //players either assigned "Red Ball" or "Yellow Ball" after fair break
    public string P1BallColour;
    public string P2BallColour;

    //UI text that is altered depending on game state / time
    public TMP_Text timerText;
    public TMP_Text playerTurnTimerText;
    public TMP_Text playerWinText;

    //bool for activating timer in update
    public bool timerOn = false;
    //checks whether player is able to shoot at the cue ball during their turn or not
    public bool canHitCueBall = false;
    //checks whether a player has taken their turn 
    public bool turnTaken = false;
    //bool for if a player has run out of time on their turn
    public bool timeRunOut = false;
    //bool that becomes false once a player pots a coloured ball
    public bool tableOpen = true;
    //activates once state moves to P1 turn for the first time
    public bool tableBallsActive = false;
    public bool scoredABall = false;

    //reference to parnent object of ball script
    public GameObject ballsManager;

    //reference to balls script
    Balls balls;

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        balls = ballsManager.GetComponent<Balls>();
        //main menu is first game state when game is opened
        UpdateGameState(GameState.MainMenuOpen);

        //sets balls on table as false before the turns start
        for (int i = 0; i < balls.BallsOnTable.Count; i++)
        {
            balls.BallsOnTable[i].SetActive(false);
        }
    }
    
    void Update()
    {
        //runs timer during a player turn
        if (timerOn)
        {
            //counts down timer before the player takes their shot at the cue ball
            if (timeLeft > 0 && turnTaken == false)
            {
                timeLeft -= Time.deltaTime;
                displayCountdown(timeLeft, timerText);
            }

            //resets timer state before each new turn
            else
            {
                timeLeft = 0;
                timerOn = false;
            }
            
            //method to handle each player turn
            handleTurn();
        }

        if (turnTaken)
        {
            //checks if balls are still moving
            balls.CheckBallMovement();
            //only updates player turn once balls have stopped moving
            UpdatePlayerTurn();
        }
    }
    
    //game state handler
    public void UpdateGameState (GameState newState)
    {
        gameState = newState;

        switch (newState)
        {
            case GameState.MainMenuOpen:
                break;

            case GameState.InitControlsMenuOpen:
                break;

            case GameState.P1Turn:
                
                //sets balls to be active from move from initial controls menu to first player 1 turn
                if(!tableBallsActive)
                {
                    for (int i = 0; i < balls.BallsOnTable.Count; i++)
                    {
                        balls.BallsOnTable[i].SetActive(true);
                    }

                    tableBallsActive = true;
                }

                //resets these bools at beginning of each turn
                turnTaken = false;
                scoredABall = false;
                balls.ballsStoppedMoving = false;

                //updates timer for start of turn
                playerTurnTimerText.text = "Player 1";
                timeLeft = 60f;
                    
                canHitCueBall = true;
                timerOn = true;

                break;

            //same logic for player 1 but reversed
            case GameState.P2Turn:
                turnTaken = false;
                scoredABall = false;

                balls.ballsStoppedMoving = false;

                playerTurnTimerText.text = "Player 2";
                timeLeft = 60f;

                canHitCueBall = true;
                timerOn = true;
                break;

            //win states for end of the game
            case GameState.P1Win:
                playerWinText.text = "Player 1 Wins";
                break;

            case GameState.P2Win:
                playerWinText.text = "Player 2 Wins";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }

    //changes UI text for timer as time counts down
    private void displayCountdown(float time, TMP_Text timerDisplay)
    {
        //displays full second until second is up, instead of milliseconds
        time += 1;
        
        //allows time to be displayed in minutes and seconds
        float mins = Mathf.FloorToInt(time / 60);
        float secs = Mathf.FloorToInt(time % 60);

        timerDisplay.text = string.Format("{0:00}:{1:00}", mins, secs);
    }

    //after a foul, gives the opposing player 2 turns
    public void awardOpponentTurn()
    {
        if(gameState == GameState.P1Turn)
        {
            P2NumOfTurns = P2NumOfTurns + 2;
            P1NumOfTurns = 0;

            UpdateGameState(GameState.P2Turn);
        }

        else if (gameState == GameState.P2Turn)
        {
            P1NumOfTurns = P1NumOfTurns + 2;
            P2NumOfTurns = 0;

            UpdateGameState(GameState.P1Turn);
        }
    }

    private void handleTurn()
    {
        if(gameState == GameState.P1Turn)
        {
            if (turnTaken)
            {
                P1NumOfTurns--;
                
                //updates the game state to player 2's turn once player 1 is out of turns
                if (P1NumOfTurns == 0)
                {
                    P2NumOfTurns++;
                }
            }

            //non-standard foul for running out of time
            if (!turnTaken && timeLeft == 0)
            {
                awardOpponentTurn();
            }
        }
        
        //same logic for player 1 reversed
        else if(gameState == GameState.P2Turn)
        {
            if (turnTaken)
            {
                P2NumOfTurns--;

                if (P2NumOfTurns == 0)
                {
                    P1NumOfTurns++;
                }
            }

            else if (!turnTaken && timeLeft == 0)
            {
                awardOpponentTurn();
            }
        }
    }

    //updates player turn states based on conditions
    private void UpdatePlayerTurn()
    {
        //only updates states once each ball on the table has stopped moving
        if(balls.ballsStoppedMoving)
        {
            //updates state to player 1 again if they have another turn
            if (gameState == GameState.P1Turn && P1NumOfTurns > 0)
            {
                UpdateGameState(GameState.P1Turn);
            }

            //updates state to player 2 turn once player 1 is out of turns
            else if (gameState == GameState.P1Turn && P1NumOfTurns == 0)
            {
                UpdateGameState(GameState.P2Turn);
            }

            //ipdates state to player 2 again if they have another turn
            else if (gameState == GameState.P2Turn && P2NumOfTurns > 0)
            {
                UpdateGameState(GameState.P2Turn);
            }

            //updates state to player 1 once player 2 is out of turns
            else if (gameState == GameState.P2Turn && P2NumOfTurns == 0)
            {
                UpdateGameState(GameState.P1Turn);
            }
        }
    } 
}


//enum for each game state
public enum GameState
{
    MainMenuOpen,
    //Controls menu leading directly to first player turn
    InitControlsMenuOpen,
    P1Turn,
    P2Turn,
    P1Win,
    P2Win,
}


