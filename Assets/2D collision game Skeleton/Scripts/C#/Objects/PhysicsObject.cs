/*
 * The definition file for the default application object.
 *
 * Part of the mechaEngine physics system.
 *
 * Copyright (c) Dwitee Krishna Panda. All Rights Reserved.
 *
 * This software is distributed under licence. Use of this software
 * implies agreement with all terms and conditions of the accompanying
 * software licence.
 */

using UnityEngine;
using System.Collections;
using System.Linq;

public class PhysicsObject : MonoBehaviour
{
    public float gravity;                                   //The pull force on the y axis
    public float springBounciness;                          //How much should the object bounce when it hits the trampoline
	public float inertia;

    protected bool canMove = false;                         //Object movement enabled/disabled 
	string[] balls = {"PfOpenMOSoccerBall","PfOpenMOTennisBall", "PfMissionMOSoccerBall", "PfMissionMOTennisBall","TrampolineTop" }; // My added prefab objects
	
	// instance members for sound
	public AudioClip _Sound;

	void Awake()
	{
		if (this.GetComponent<Rigidbody2D>())
			this.rigidbody2D.inertia = inertia;
	}
	/// <summary>
	/// Plaies the sound.
	/// </summary>
	/// <param name="inForce">If set to <c>true</c> in force.</param>
	protected void PlaySound(bool inForce)
	{
//		if (_Sound != null)
//			SnChannel.Play(_Sound, "", inForce); 	
	}
	
	/// <summary>
	/// Plaies the sound.
	/// </summary>
	protected void PlaySound()
	{
//	if (_Sound != null)
//		SnChannel.Play(_Sound, "", false); 	
	}
	
    //Called when the object collides with an other object
    public virtual void OnCollisionEnter2D(Collision2D other)
    {

        //If we hit another object which has an ObjectBase script, notify the goal manager
        if (GetParent(other.transform))
            GoalManager.Instance.CollisionEvent(this.gameObject, GetParent(other.transform).gameObject);

        //If we hit a trampoline, bounce back
        if (other.collider.gameObject .name == "TrampolineTop")
            this.rigidbody2D.AddForce(Vector2.up * rigidbody2D.velocity.y * springBounciness);

        //If the y velocity is smaller than 1.25, make the object stop vertically
        if (Mathf.Abs(this.rigidbody2D.velocity.y) < 1.25f)
            this.rigidbody2D.velocity = new Vector2(this.rigidbody2D.velocity.x, 0);
		
		//only balls plays sounds on collinding when dropped from a height
		if(( balls.Contains( this.rigidbody2D.name)) && other.relativeVelocity.sqrMagnitude > 25.0f )
			PlaySound(true);
    }
    //Called when the object enters a trigger zone
    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        //If we hit another object which has an ObjectBase script, notify the goal manager
        if (GetParent(other.transform))
            GoalManager.Instance.TriggerEvent(this.gameObject, GetParent(other.transform).gameObject);
        //If we hit the empty target object, notify the goal manager
        else if (other.name == "EmptyTarget")
            GoalManager.Instance.TriggerEvent(this.gameObject, other.gameObject);
    }

    //Called when the level enters play mode
    public virtual void Enable()
    {
        canMove = true;
        this.rigidbody2D.gravityScale = gravity;
        this.rigidbody2D.fixedAngle = false;
    }
    //Called when the level leaves play mode
    public virtual void Reset()
    {
        canMove = false;
        this.rigidbody2D.gravityScale = 0;
        this.rigidbody2D.velocity = new Vector2(0, 0);
        this.rigidbody2D.fixedAngle = true;
		//stop all  physics sound in planing mode 
//		SnChannel.StopPool("");
    }

    //Returns the ObjectBase parent of the item
    protected ObjectBase GetParent(Transform item)
    {
        while (item != null && item.GetComponent<ObjectBase>() == null)
            item = item.parent;

        if (item == null)
            return null;
        else
            return item.GetComponent<ObjectBase>();
    }
}