using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakeTurn : MonoBehaviour
{
    //power applied to ball when moving
    public float power;

    //minimum power ball can be dragged back with
    public Vector2 minPower;
    //maximum power ball can be dragged back with
    public Vector2 maxPower;
    //gives force added to ball when multiplied by power
    Vector2 force;

    //location of where mouse was first clicked
    Vector3 startPoint;
    //location of where mouse is dragged to once clicked
    Vector3 endPoint;

    //gets reference to the scene's camera
    Camera cam;
    
    //calling referernce to ball's rigidbody component
    public Rigidbody2D rb;
    //calling reference to the cue ball's collider component
    public CircleCollider2D cc;

    //reference to game manager script
    public GameManager gm;

    //referenc to cue ball's line renderer component for the aim line
    public LineRenderer lr;

    //reference to the layermast to ensure the aim line only collides with other balls / the table cushions
    public LayerMask lm;

    private void Start()
    {
        //gets access to the components
        cam = Camera.main;
        lr = GetComponent<LineRenderer>();
        cc = GetComponent<CircleCollider2D>();
    }
    
    void OnMouseDown()
    {
      //transforms start point variable into world position
      startPoint = transform.position;
      //prevents z axis variable being hidden behind other elements in the scene
      startPoint.z = 15;
    }

    void OnMouseDrag()
    {
        //gets mouse position at each moment of mouse drag
        Vector3 currPoint = cam.ScreenToWorldPoint(Input.mousePosition);
        //prevents z axis variable being hidden behind other elements in the scene
        currPoint.z = 15;
        //renders aim line for players using the cue ball's start point and the mouse position as context
        RenderLine(startPoint, currPoint); 
    }
    
    void OnMouseUp()
    {
        //method only works if players can take their turn, doesn't allow players to hit the cue ball when they shouldn't
        if (gm.canHitCueBall)
        {
            //transforms end point variable into world position
            endPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            //prevents z axis variable being hidden behind other elements in the scene
            endPoint.z = 15;

            //calculates necessary force behind cue ball's movement
            force = new Vector2(Mathf.Clamp(startPoint.x - endPoint.x, minPower.x, maxPower.x), Mathf.Clamp(startPoint.y - endPoint.y, minPower.y, maxPower.y));
            //impuse adds instant force instead of gradual force
            rb.AddForce(force * power, ForceMode2D.Impulse);
            
            //prevents cue ball from being hit twice in same turn
            gm.canHitCueBall = false;
            gm.turnTaken = true;

            //stops the aim line from appearing on screen
            StopLineShowing();
        }
    }

    //aim line method
    public void RenderLine(Vector3 startPos, Vector3 currPos)
    { 
        //aim line has 2 points: start point from cue ball and end point where collision is
        lr.positionCount = 2;
        
        //direction aim line should be created in
        Vector3 direction = ((currPos - startPos) * -1);

        //raycast detecting collisions within the layermask for the aim line
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, Mathf.Infinity, lm);

        //if the raycast collides with a ball or cushion
        if(hit.collider != null)
        {
            Vector2 hitPoint = hit.point;
        }
        
        //draws aim line 
        Vector3[] points = new Vector3[2];
        points[0] = startPos;
        points[1] = hit.point;

        lr.SetPositions(points);
    }
    
    //deactivates aim line
    public void StopLineShowing()
    {
        lr.positionCount = 0;
    }
}
