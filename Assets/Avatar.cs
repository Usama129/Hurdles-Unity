﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using UnityEngine;

public class Avatar : MonoBehaviour
{
    private Animator jumpAnim;
    private GameObject ground;
    private float jumpForce = 5;
    public static bool kinectJump, kinectRight, kinectLeft, kinectCrouch, kinectStandFromCrouch;
    public static Vector3 pos;
    private int lane = 1;
    private float maxRight, maxLeft, crouchStartTime = 0, crouchingTime = 2;
    private Stopwatch lastSwerve = new Stopwatch();

    void Start()
    {
        ground = GameObject.Find("Ground");
        jumpAnim = GetComponent<Animator>();
        kinectJump = false;
        kinectRight = false;
        kinectLeft = false;
        kinectCrouch = false;
        kinectStandFromCrouch = false;
        maxRight = 4.3f;
        maxLeft = -4.3f;
    }

    void FixedUpdate()
    {

        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0f);


        Move();

        if (currentStateMatches("Crouch"))
        {
            crouchStartTime += Time.deltaTime;
        }

        pos = GetComponent<Rigidbody>().position;

        if ((Input.GetKeyDown(KeyCode.UpArrow) || kinectJump) && IsGrounded())
        {

            if (currentStateMatches("Crouch"))
            {
                StandFromCrouch();
            }
            else
            {

                GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpAnim.Play("Start Jump");
                kinectJump = false;
            }
        }


        if (DistanceToGround() < 1 && VerticalVelocity() < 0)
        {
            jumpAnim.Play("End Jump");
        }

        if ((Input.GetKeyDown(KeyCode.RightArrow) || kinectRight) && IsGrounded())
        {
            ChangeLane(true);
            kinectRight = false;
        }

        if ((Input.GetKeyDown(KeyCode.LeftArrow) || kinectLeft) && IsGrounded())
        {
            ChangeLane(false);
            kinectLeft = false;
        }

        if ((Input.GetKey(KeyCode.DownArrow) || kinectCrouch) && IsGrounded())
        {
            Crouch();
            kinectCrouch = false;
        }

        if (crouchStartTime > crouchingTime)
            StandFromCrouch();
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.name.Contains("Asteroid"))
        {
            jumpAnim.Play("Death");
        }
    }

    private float DistanceToGround()
    {
        return GetComponent<Rigidbody>().position.y - ground.transform.position.y;
    }

    private bool IsGrounded()
    {
        return DistanceToGround() < 0.6; // When Avatar is on the ground, DistanceToGround() returns around 0.54
    }

    private float VerticalVelocity()
    {
        return GetComponent<Rigidbody>().velocity.y;
    }

    private bool currentStateMatches(string state) // checks if a state matches the current state in the Jump Animator
    {
        return jumpAnim.GetCurrentAnimatorStateInfo(0).IsName(state);
    }

    private void Crouch()
    {
        jumpAnim.Play("Crouch");


        //UnityEngine.Debug.Log("Crouch");
        /* GetComponent<CapsuleCollider>().height = 1.5f;
         GetComponent<Rigidbody>().AddForce(Vector3.down * 1, ForceMode.Impulse);*/

    }

    private void StandFromCrouch()
    {

        jumpAnim.Play("Idle");
        crouchStartTime = 0;


    }

    private void ChangeLane(bool towardRight)
    {
        if (lastSwerve.ElapsedMilliseconds > 0 && lastSwerve.ElapsedMilliseconds < 1500)
            return;
        //UnityEngine.Debug.Log(lastSwerve.ElapsedMilliseconds);
        lane += towardRight ? 1 : -1;
        lane = Mathf.Clamp(lane, 0, 2);
        lastSwerve.Restart();
    }

    private void Move()
    {
        float step = 25.0f * Time.deltaTime; // calculate distance to move
        Vector3 target = transform.position;
        if (lane == 0)
            target.x = maxLeft;
        else if (lane == 1)
            target.x = 0.0f;
        else if (lane == 2)
            target.x = maxRight;
        transform.position = Vector3.MoveTowards(transform.position, target, step);
    }

}