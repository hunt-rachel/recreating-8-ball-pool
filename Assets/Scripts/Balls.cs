using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balls : MonoBehaviour
{
    //reference to parent object of game manager script
    public GameManager gm;
    
    //list keeping reference to all balls remaining on the table
    public List<GameObject> BallsOnTable = new List<GameObject>();

    //lists keeping reference to the balls each player has potted
    //only keeps reference to their associated colour balls
    public List<GameObject> P1PottedBalls = new List<GameObject>();
    public List<GameObject> P2PottedBalls = new List<GameObject>();

    //array of game objects representing the balls in the scoreboards
    public GameObject[] P1ScoreboardBalls;
    public GameObject[] P2ScoreboardBalls;

    //float for fading out coroutine
    public float fadeLength;

    //array of positions each ball is stacked at
    //in case balls need to be restaced after a non-fair break
    [SerializeField] private Vector2[] RestackPositions = new Vector2[16];

    public bool ballsStoppedMoving = false;
    
    void Awake()
    {
        //finds initial positions for balls to be restacked to if needs be
        for (int i = 0; i < BallsOnTable.Count; i++)
        {
            RestackPositions[i] = BallsOnTable[i].transform.position;
        }
    }

    void Update()
    {
        //stops ball movement fully once movement becomes too minimal
        //otherwise checking for stopped ball movement would take longer than is realistic
        for (int i = 0; i < BallsOnTable.Count; i++)
        {
            Rigidbody2D rb = BallsOnTable[i].GetComponent<Rigidbody2D>();
            if (rb.velocity.magnitude < 0.05f)
            {
                StopBallMovement(rb);
            }
        }
    }
    
    //restacks all balls in case of non-fair breal
    public void RestackBalls()
    {
        for(int i = 0; i < BallsOnTable.Count; i++)
        {
            //gets reference to each ball in the list's rigidbody2D component
            Rigidbody2D rb = BallsOnTable[i].GetComponent<Rigidbody2D>();
            StopBallMovement(rb);
            //resets balls to stacked locations
            BallsOnTable[i].transform.position = new Vector2(RestackPositions[i].x, RestackPositions[i].y);
            //fades balls back in
            StartCoroutine(FadeInAnim(BallsOnTable[i]));
        }
    }

    //only restacks cue ball
    public void RestackCue()
    {
        BallsOnTable[0].transform.position = new Vector2(RestackPositions[0].x, RestackPositions[0].y);
        StartCoroutine(FadeInAnim(BallsOnTable[0]));
    }

    //only restacks 8 ball
    public void Restack8Ball()
    {
        BallsOnTable[1].transform.position = new Vector2(RestackPositions[1].x, RestackPositions[1].y);
        StartCoroutine(FadeInAnim(BallsOnTable[1]));
    }

    //stops ball movement completely
    void StopBallMovement(Rigidbody2D rb)
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    //checks each ball on the table's velocity to see if it is still moving or not
    public void CheckBallMovement()
    {
        for (int i = 0; i < BallsOnTable.Count; i++)
        {
            Rigidbody2D rb = BallsOnTable[i].GetComponent<Rigidbody2D>();
            if (rb.velocity.magnitude != 0f)
            {
                ballsStoppedMoving = false;
                break;
            }

            else
            {
                ballsStoppedMoving = true;
            }
        }
    }

    //enumerator for balls fading out
    public IEnumerator FadeOutAnim(GameObject ball)
    {
        Color opaqueBall = ball.GetComponent<Renderer>().material.color;
        Color transparentBall = new Color(opaqueBall.r, opaqueBall.g, opaqueBall.b, 0f);

        float timeElapsed = 0f;

        while (timeElapsed < fadeLength)
        {
            timeElapsed += Time.deltaTime;
            ball.GetComponent<Renderer>().material.color = Color.Lerp(opaqueBall, transparentBall, timeElapsed / fadeLength);
            yield return null;
        }
    }

    //enumerator for balls fading in
    public IEnumerator FadeInAnim(GameObject ball)
    {
        Color transparentBall = ball.GetComponent<Renderer>().material.color;
        Color opaqueBall = new Color(transparentBall.r, transparentBall.g, transparentBall.b, 0f);

        float timeElapsed = 0f;

        while (timeElapsed < fadeLength)
        {
            timeElapsed += Time.deltaTime;
            ball.GetComponent<Renderer>().material.color = Color.Lerp(transparentBall, opaqueBall, timeElapsed / fadeLength);
            yield return null;
        }
    }
}
