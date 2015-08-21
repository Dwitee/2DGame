
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
using System.Collections.Generic;

public class LevelDesignManager : MonoBehaviour 
{
    public string levelNumber;                                      //The current level number

    public Transform activeContainer;                               //Holds the level elements
    public Transform BackPackContainer;                              //Hold the toolbar elements at start
	public AudioClip _SoundGoalReached;
    protected static LevelDesignManager myInstance;                                 //Hold a reference to this script

    protected List<ObjectBase> activeObjects;                                 //Hold every active object
    List<BackPackItemType> BackPackItemTypes;                         //Hold every toolbar object

    protected int starsCollected = 0;                               //Number of collected stars
	protected bool goalReached = false;                             //Goal reached/not reached
	protected bool inPlayMode = false;                              //The level is in play mode/plan mode

    public static LevelDesignManager Instance { get { return myInstance; } }

    //Called at the start of the level
	protected virtual void Start()
	{
        myInstance = this;

        //Create the lists
        activeObjects = new List<ObjectBase>();
        BackPackItemTypes = new List<BackPackItemType>();

        //Loop trough the active objects and add them to the active list, if they have an ObjectBase component
        foreach (Transform item in activeContainer)
        {
            if (item.GetComponent<ObjectBase>())
            {
                activeObjects.Add(item.GetComponent<ObjectBase>());
                item.GetComponent<ObjectBase>().Setup();
            }
        }

        //Loop trough the BackPack objects and add them to the BackPack list
        foreach (Transform item in BackPackContainer)
        {
            item.GetComponent<BackPackItemType>();
            BackPackItemTypes.Add(item.GetComponent<BackPackItemType>());
        }

        //And send them to the BackPack manager
        BackPackManager.Instance.SetupToolbar(BackPackItemTypes);
	}

    //Enable the scene
    public virtual void EnableScene()
    {
        inPlayMode = true;

        foreach (ObjectBase item in activeObjects)
            item.Enable();
    }
    //Resets the scene
    public virtual void ResetScene()
    {
        inPlayMode = false;
        starsCollected = 0;
        
        foreach (ObjectBase item in activeObjects)
            item.Reset();
    }

    //Restart the scene
    public virtual void RestartScene()
    {
        inPlayMode = false;

        BackPackManager.Instance.ClearBackPack();
        BackPackManager.Instance.SetupToolbar(BackPackItemTypes);
        GUIManager.Instance.Restart();
    }
    //Restarts the scene from the BackPack
    public void RestartFromFinish()
    {
        inPlayMode = false;

        ResetScene();
        goalReached = false;
        GoalManager.Instance.ResetGoal();
    }

    //Called when a star is collected
    public void StarCollected()
    {
        starsCollected++;
    }
    //Returns the number of collected stars
    public int GetStarsCollected()
    {
        return starsCollected;
    }
    //Called by the goal manager, when the goal is reached by the target
    public void GoalReached()
    {
        if (!goalReached)
        {
			if(_SoundGoalReached != null)
//			SnChannel.Play(_SoundGoalReached,"SFX_Pool",true);
            goalReached = true;
//			UiCraftTraptionsResult.pInstance.ShowResultScreen();

            //GUIManager.Instance.GoalReached();
        }
    }

    //Adds an item to the active object list
    public void AddItem(ObjectBase item)
    {
        if (!activeObjects.Contains(item))
            activeObjects.Add(item);

        item.transform.parent = activeContainer;
    }
    //Removes an item from the active object list
    public virtual void RemoveItem(ObjectBase item)
    {
        activeObjects.Remove(item);
    }
    //Saves the current level data
	public virtual void SaveData()
    {
        if (PlayerPrefs.GetInt(levelNumber) < starsCollected)
        {
            PlayerPrefs.SetInt(levelNumber, starsCollected);
            PlayerPrefs.Save();
        }
    }

    //Returns true if the level is in play mode
    public bool InPlayMode()
    {
        return inPlayMode;
    }
}
