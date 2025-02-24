using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pockets : MonoBehaviour
{
    //reference for the table pocket
    Collider2D pocket;

    //reference to balls scrips
    Balls balls;

    //reference to parent object of balls script
    public GameObject ballsManager;
    //reference to parent object of game manager script
    public GameManager gm;

    //references to cue and 8 ball game objects
    public GameObject cueBall;
    public GameObject eightBall;
    

    //get components
    void Start()
    {
        balls = ballsManager.GetComponent<Balls>();
        pocket = this.GetComponent<Collider2D>();
    }
    
    //when a ball collides with a pocket, as pockets are trigger collisions
    void OnTriggerEnter2D (Collider2D pocket)
    {
        //stops ball from moving once it "falls" into the pocket
        pocket.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        //conditions for a break, when table is open
        if (gm.tableOpen)
        {
            //checks it was a colour ball that was potted
            if(pocket.gameObject != cueBall && pocket.gameObject != eightBall)
            {
                //closes table
                gm.tableOpen = false;
                Debug.Log("table closed");

                //player 1 turn logic
                if(gm.gameState == GameState.P1Turn)
                {
                    //prevents duplicate game objects being added to list
                    if(!balls.P1PottedBalls.Contains(pocket.gameObject))
                    {
                        balls.P1PottedBalls.Add(pocket.gameObject);
                    }

                    //process of fading out ball and making ball game object not active in game scene
                    PocketBall(pocket.gameObject);

                    //assigns colours to players after fair break
                    if (pocket.gameObject.tag == "Yellow Ball") 
                    {
                        gm.P1BallColour = "Yellow Ball";
                        gm.P2BallColour = "Red Ball";
                    }

                    else if (pocket.gameObject.tag == "Red Ball") 
                    {
                        gm.P1BallColour = "Red Ball";
                        gm.P2BallColour = "Yellow Ball";
                    }

                    //changes greyed out balls in scoreboard to associated colour ball based on score
                    UpdateScoreBoard(balls.P1ScoreboardBalls[balls.P1PottedBalls.Count - 1], pocket.gameObject);
                    Debug.Log("p1 scored a ball");
                    gm.scoredABall = true;
                    //awards extra turn to player 1 for potting a colour ball
                    gm.P1NumOfTurns = 1;
                    gm.P2NumOfTurns = 0;
                }

                //everything same logic as for player 1
                else if (gm.gameState == GameState.P2Turn)
                {
                    if (!balls.P2PottedBalls.Contains(pocket.gameObject))
                    {
                        balls.P2PottedBalls.Add(pocket.gameObject);
                    }
                    
                    PocketBall(pocket.gameObject);

                    if (pocket.gameObject.tag == "Yellow Ball")
                    {
                        gm.P2BallColour = "Yellow Ball";
                        gm.P1BallColour = "Red Ball";
                    }

                    else if (pocket.gameObject.tag == "Red Ball")
                    {
                        gm.P2BallColour = "Red Ball";
                        gm.P1BallColour = "Yellow Ball";
                    }

                    UpdateScoreBoard(balls.P2ScoreboardBalls[balls.P2PottedBalls.Count - 1], pocket.gameObject);
                    Debug.Log("p2 scored a ball");
                    gm.scoredABall = true;
                    gm.P2NumOfTurns = 1;
                    gm.P1NumOfTurns = 0;
                }
            }

            //handles if turn was a non-fair break
            else if(pocket.gameObject == cueBall || pocket.gameObject == eightBall)
            {
                //non-standard foul on break
                //call this function twice, otherwise it rewards the player with two extra turns for committing a foul
                gm.awardOpponentTurn();
                gm.awardOpponentTurn();
               //restacks balls as per rule of non-standard foul during break
                balls.RestackBalls();
            }
        }

        //if cue ball was potted outside of break
        if (pocket.gameObject == cueBall)
        {
            //non-standard foul on break
            gm.awardOpponentTurn();
            //only cue ball restacked
            balls.RestackCue();
        }

        else if(gm.gameState == GameState.P1Turn)
        { 
            //if player pots their own coloured ball
            //first condition prevents dupluicate game objects being added to list
            if (!balls.P1PottedBalls.Contains(pocket.gameObject) && pocket.gameObject.tag == gm.P1BallColour)
            {
                PocketBall(pocket.gameObject);
                balls.P1PottedBalls.Add(pocket.gameObject);

                //updates player 1's scoreboard to display a new coloured ball for the ball potted
                UpdateScoreBoard(balls.P1ScoreboardBalls[balls.P1PottedBalls.Count - 1], pocket.gameObject);
                gm.scoredABall = true;
                //rewards player 1 with extra turn for potting their coloured ball
                gm.P1NumOfTurns = 1;
                gm.P2NumOfTurns = 0;
            }

            //player 1 potted player 2's coloured ball
            else if (pocket.gameObject.tag == gm.P2BallColour)
            {
                balls.RestackCue();
                gm.awardOpponentTurn();
            }

            else
            {
                //if 8 ball is potted before a player has potted all their colour balls
                if (balls.P1PottedBalls.Count < 7 && pocket.gameObject == eightBall)
                {
                    //non-standard foul on break
                    Debug.Log("foul - potted 8 ball when player colour balls were on the table");
                    gm.awardOpponentTurn();
                    //only 8 balls needs to be restacked
                    balls.Restack8Ball();
                }

                //if player pots 8 ball after potting all their coloured balls
                else if(balls.P1PottedBalls.Count == 7 && pocket.gameObject == eightBall)
                {
                    UpdateScoreBoard(balls.P1ScoreboardBalls[balls.P1PottedBalls.Count - 1], pocket.gameObject);
                    gm.UpdateGameState(GameState.P1Win);
                }
            }
        }

        //same logic as player 1, but reversed for player 2
        else if(gm.gameState == GameState.P2Turn)
        {
            if (!balls.P2PottedBalls.Contains(pocket.gameObject) && pocket.gameObject.tag == gm.P2BallColour)
            {
                PocketBall(pocket.gameObject);
                balls.P2PottedBalls.Add(pocket.gameObject);

                UpdateScoreBoard(balls.P2ScoreboardBalls[balls.P2PottedBalls.Count - 1], pocket.gameObject);
                gm.scoredABall = true;
                gm.P2NumOfTurns = 1;
                gm.P1NumOfTurns = 0;
            }

            else if (pocket.gameObject.tag == gm.P1BallColour)
            {
                balls.RestackCue();
                gm.awardOpponentTurn();

            }

            else
            {
                //if 8 ball is potted before a player has potted all their colour balls
                if (balls.P1PottedBalls.Count < 7 && pocket.gameObject == eightBall)
                {
                    //non-standard foul on break
                    Debug.Log("foul - potted 8 ball when player colour balls were on the table");
                    gm.awardOpponentTurn();
                    //only 8 balls needs to be restacked
                    balls.Restack8Ball();
                }

                if (balls.P2PottedBalls.Count == 7 && pocket.gameObject == eightBall)
                {
                    UpdateScoreBoard(balls.P2ScoreboardBalls[balls.P2PottedBalls.Count - 1], pocket.gameObject);
                    gm.UpdateGameState(GameState.P2Win);
                }
            }
        }
        
    }

    //updates the table when a ball is potted
    void PocketBall(GameObject ball)
    {
        StartCoroutine(balls.FadeOutAnim(ball));
        balls.BallsOnTable.Remove(ball);
        ball.SetActive(false);
    }

    //updates the scoreboard visuals 
    void UpdateScoreBoard(GameObject greyBall, GameObject colourBall)
    {
        greyBall.GetComponent<SpriteRenderer>().sprite = colourBall.GetComponent<SpriteRenderer>().sprite;
    }
}
