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

public class GoalManager : MonoBehaviour 
{
    public GameObject target;
    public GameObject goal;
	public GameObject targetB;
	public GameObject goalB;

    public bool onTrigger;

    static GoalManager myInstance;
    public static GoalManager Instance { get { return myInstance; } }
	public bool GoalAReached;
	public bool GoalBReached;
	
    void Start()
    {
        myInstance = this;
    }

	public void ResetGoal()
	{
		GoalAReached = false;
		GoalBReached = false;
	}
	public bool IsMultipleGoal()
	{
		if(targetB !=null && goalB !=null)
			return true;
		else
			return false;	
			
	}
    //Called when an object collides with another object
    public void CollisionEvent(GameObject sender, GameObject collidedWith)
    {
		if (IsMultipleGoal())
    	{
			if ((sender == target && collidedWith == goal && !onTrigger) || (sender == targetB && collidedWith == goal && !onTrigger))
				GoalAReached = true;
				
			if ((sender == target && collidedWith == goalB && !onTrigger) || (sender == targetB && collidedWith == goalB && !onTrigger))
				GoalBReached = true;
				
			if ( GoalAReached ==true && GoalBReached ==true)
				LevelDesignManager.Instance.GoalReached();		
    	}
    	else
        {
        
        	if (sender == target && collidedWith == goal && !onTrigger)
            	LevelDesignManager.Instance.GoalReached();
         }
    }
    //Called when an object enters a trigger zone
    public void TriggerEvent(GameObject sender, GameObject triggeredWith)
    {
		if (IsMultipleGoal())
		{
			if((sender == target && triggeredWith == goal && onTrigger) || (sender == targetB && triggeredWith == goal && onTrigger))
				GoalAReached = true;
				
			if((sender == target && triggeredWith == goalB && onTrigger) || (sender == targetB && triggeredWith == goalB && onTrigger))
				GoalBReached = true;	
		}
        else
        {
        	if(sender == target && triggeredWith == goal && onTrigger)
            	LevelDesignManager.Instance.GoalReached();
        }
    }
    //Called when a ObjBalloon explodes
    public void ObjBalloonExloded(GameObject sender)
    {
        if (sender == target && goal == null)
            LevelDesignManager.Instance.GoalReached();
    }
}
