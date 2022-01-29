﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState{
	Controllable,
	Transition1,
	Transition2,
	Transition3,
	Transition4,
	Transition5,
	Dying
}

public class PlayerController : MonoBehaviour
{

	public Transform startPoint;
	[Space(6)]

	[Header("Movement")]
	public float jumpPower = 100;
    public float moveSpeed = 10;
    public float accelTime = 0.1f;
    public bool preJump = false;
    public bool canJump = false;
	public bool canFlip = false;
	public bool landing = false;
    public float jumpCD = 0f;
	public Rigidbody2D phys;
	public bool canMove = true;
	[Space(8)]

	[Header("Animation")]
	public float deathTime = 1.0f;
	public float deathTimer = 0.0f;
	public AnimationClip preJumpAnimation = null;
	private float preJumpAnimLength = 0.0f; //set this to the linked preJumpAnimation's duration when the script starts
	private float preJumpAnimStart = 0.0f; //will mark the start point for the timer to go off after the duration of the jump animation time
	public Animator anim;
	
	//Gamedriver and State
	private GameDriver gameDriver;
	public PlayerState state = PlayerState.Controllable;




	//button buffer
	private bool selectIsDown = false;

	//animation
	/*private Vector3 transitionStartPos;
	private float transTimer;
	private float lerpRate = 1f;
	private Vector3 transitionEndPos;
	private bool doneMoving = true;
	private float transitionTravelDis;
	private float scaleFactor = 1f;
	private float deathTimer = 0f;*/

	private int ignoreExit = 0;
    


    // Use this for initialization
    void Awake()
    {
		//GameObject.DontDestroyOnLoad (gameObject);

		if (startPoint != null) {
			this.transform.position = startPoint.position;
		}

		gameDriver = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameDriver> ();
		if (preJumpAnimation) preJumpAnimLength = preJumpAnimation.length;
        //phys = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

	// Update is called once per screen frame.
	void Update(){
		switch (state) {
		case PlayerState.Controllable:
			if (Input.GetAxis ("Interact") > 0) {
				if (!selectIsDown) {
					Debug.Log ("KeyPressed");
					selectIsDown = true;
				}
			} else {
				selectIsDown = false;
			}
			break;

		case PlayerState.Dying:
			if (deathTimer > 0) {
				gameObject.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.white, deathTimer%0.5f);
				deathTimer -= Time.deltaTime;
			} else {
				gameDriver.ReloadLevel ();
			}
			break;

		}
	}

    // FixedUpdate is called once per physics(??) frame; much more well regulated intervals than Update
    void FixedUpdate()
    {
		if (state == PlayerState.Controllable)
        {
            if(phys.velocity.x>0)GetComponent<SpriteRenderer>().flipX = false;
            if (phys.velocity.x < 0) GetComponent<SpriteRenderer>().flipX = true;
            if (jumpCD > 0) jumpCD--;
			


			if (phys.velocity.x < 0)
			{
				if (Input.GetAxis("Horizontal") < 0)
				{
					phys.AddForce(new Vector2(Input.GetAxis("Horizontal") * moveSpeed, 0), ForceMode2D.Force);
					if (canJump) anim.SetInteger("animState", 1);//walk
				}
				else if (Input.GetAxis("Horizontal") > 0)
				{
					phys.AddForce(new Vector2(Input.GetAxis("Horizontal") * moveSpeed + phys.velocity.x*phys.mass*-5f, 0), ForceMode2D.Force);
					if (canJump) anim.SetInteger("animState", 1);//walk
				}
				else if (Input.GetAxis("Horizontal")==0)
				{
					phys.AddForce(new Vector2(phys.velocity.x*phys.mass*-20f,0), ForceMode2D.Force);
					anim.SetInteger("animState", 0);
				}
			}
			else if (phys.velocity.x > 0)
			{
				if (Input.GetAxis("Horizontal") < 0)
				{
					phys.AddForce(new Vector2(Input.GetAxis("Horizontal") * moveSpeed + phys.velocity.x * phys.mass * -5f, 0), ForceMode2D.Force);
					if (canJump) anim.SetInteger("animState", 1);//walk
				}
				else if (Input.GetAxis("Horizontal") > 0)
				{
					phys.AddForce(new Vector2(Input.GetAxis("Horizontal") * moveSpeed, 0), ForceMode2D.Force);
					if (canJump) anim.SetInteger("animState", 1);//walk
				}
				else if (Input.GetAxis("Horizontal") == 0)
				{
					phys.AddForce(new Vector2(phys.velocity.x * phys.mass * -20f, 0), ForceMode2D.Force);
					anim.SetInteger("animState", 0);//idle
				}
			}
			else if (phys.velocity.x == 0 && Input.GetAxis("Horizontal")!=0)
			{
				phys.AddForce(new Vector2(Input.GetAxis("Horizontal") * moveSpeed*20, 0), ForceMode2D.Force);
				if (canJump) anim.SetInteger("animState", 1);//walk
			}
			else if (phys.velocity.x==0 && Input.GetAxis("Horizontal")==0)
			{
				if (canJump) anim.SetInteger("animState", 0);//idle
			}

			phys.velocity = Vector3.ClampMagnitude(phys.velocity, moveSpeed);
			//phys.velocity = new Vector2(Mathf.Lerp(phys.velocity.x, Input.GetAxis("Horizontal"), accelTime) * moveSpeed, phys.velocity.y);
			

			//Now for the jumping part of the code!
			if (Input.GetAxis("Vertical") > 0)
			{
				if (canJump && jumpCD==0)
				{
					if (preJump == false)
					{
						preJump = true;
						//Debug.Log("Jump Pressed, jumping prep");
						anim.SetInteger("animState", 3);//ascend
						//Debug.Log("Anim State: " + anim.GetInteger("animState"));
						//Do the prejump animation
					}
					else
                    {
						//phys.AddForce(Vector2.up * jumpCD); //This is how to do the jump force, need to find a place to put it
					}
				}
			}
			if (preJump == true)
			{
				//need to mess with prejump stuff
				canJump = false;
				jumpCD = 30;
				phys.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
				anim.SetInteger("animState", 3);//ascend
												//Debug.Log("Should be 3: " + anim.GetInteger("animState"));
												//Do the jump animation
			}
			else if(Input.GetAxis("Vertical")<=0)
			{
				jumpCD = 0;
		}
		}
		if(phys.velocity.y<0)
		{
			phys.AddForce(new Vector2(0, -18f * phys.mass));
			//Debug.Log("Falling, trigger falling anim");
			anim.SetInteger("animState", 4);//descend
			//Do the falling animation
		}
	}

	/*public void SetSwitch(EmiterSwitch targetSwitch){
		Debug.Log ("Over switch");
		overSwitch = true;
        
		eSwitch = targetSwitch;
	}*/

	/*public void LeaveSwitch(){
		Debug.Log ("Leaveswitch");
		overSwitch = false;
		eSwitch = null;
	}*/

	/*public void SetInSignal(bool isIn){
		if (inWifiRange && isIn) {
			ignoreExit++;
		} else {
			inWifiRange = isIn;
			if (!inTransitionRange) {
				if (isIn) {
					this.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
                    GetComponent<Rigidbody2D>().gravityScale = 0.1f;
                } else {
					this.gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
                    GetComponent<Rigidbody2D>().gravityScale = 1;
                }
			}
		}
	}*/

	/*public void SetInTransitionRange(bool isIn, TransitionDish targetDish){
		inTransitionRange = isIn;
		if (isIn) {
			dish = targetDish;
		} else if (inWifiRange) {
			this.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
		} else {
			this.gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
		}
	}*/

	public void Die(){
		phys.velocity = Vector2.zero;
		phys.gravityScale = 0;
		deathTimer = deathTime;
		state = PlayerState.Dying;
	}

	private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer==9)
        {
            bool isAbove = false;
            ContactPoint2D[] points = collision.contacts;
            foreach(ContactPoint2D point in points)
            {
                if(point.point.y<(transform.position.y-GetComponent<SpriteRenderer>().bounds.extents.y*0.5f))
                {
                    //Debug.Log("The point: " + point.point + " Is below: " + (transform.position.y - GetComponent<SpriteRenderer>().bounds.extents.y * 0.9f));
                    isAbove = true;
                    canJump = true;
                    if(Input.GetAxis("Horizontal")!=0)
                    {
                        //anim.SetInteger("animState", 1);//walk
                    }
                    //phys.velocity = Vector2.zero;
                    //anim.SetInteger("animState", 0);
                    //jumpCD = 0;
                }
            }
            //canJump = true;
            //jumpCD = 0;
            //Do the landing animation
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        //Debug.Log("Collision Exit");
        if (!GetComponent<CapsuleCollider2D>().IsTouchingLayers(512))
        {
            //Debug.Log("Not touching ground, can't jump.");
            canJump = false;
            //anim.SetInteger("animState", 4);//descend
        }
    }
}
