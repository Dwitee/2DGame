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
using GameOfCollision;
public class InputManager : MonoBehaviour 
{
    enum InputState { scrolling, moving, rotating, waitingForInput };   //The possible states for the BackPack
    
    public LayerMask mask = -1;					            //Set input layer mask
    public bool useTouch = true;                                   //Use touch controls

    static InputManager myInstance;                         //Hold a reference to this script

    InputState inputState = InputState.waitingForInput;     //The state of the input

    RaycastHit2D hit;                                       //The raycast to detect the targetet item

    Transform selectedItem;                                 //The transform of the selected item
    ObjectBase selectedObject;                              //The ObjectBase script of the selected item

    float scrollStartingPos;                                //The starting position to the BackPack scrolling

    bool itemSelectionValid = false;                        //Selected item valid/invalid for dragging/rotating
    bool hasFeedback = false;                               //Feedback activated/deactivated

    Vector3 inputPos;                                       //The position of the input
    Vector3 offset;                                         //The starting offset between the selected item and the input position
    Vector3 startingRot;                                    //The starting rotation of the item
    Vector2 startingVector;                                 //The starting vector of the rotation (inputPos - selectedItem.position)
    Vector2 currentVector;                                  //The current vector of the rotation (inputPos - selectedItem.position)
	Vector2 lastPosition;
	bool fromBackPack = false;
		
	private bool mItemSelected;
	public bool pItemSlected
	{
		get { return mItemSelected; }
		set { mItemSelected = value; }
	}

    //Returns the instance
    public static InputManager Instance { get { return myInstance; } }

    //Called at the start of the level
    void Start()
    {
        myInstance = this;
        offset = Vector3.zero;
    }
    //Called at every frame, tracks input
    void Update()
    {
	//	useTouch = UtUtilities.IsMobilePlatform();

        if (useTouch)
            TouchControls();
        else
            MouseControls(); 
    }
    //Mouse controls
    void MouseControls()
    {
        //Get the position of the input
        inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        inputPos.z = 0;

        //Cast a ray to detect objets
        hit = Physics2D.Raycast(inputPos, new Vector2(0, 0), 0.1f, mask);

        if (inputState == InputState.waitingForInput)
        {
            ScanForInput();
        }
        else if (inputState == InputState.scrolling)
        {
            ScrollBackPack();

            //If the input was released
            if (Input.GetMouseButtonUp(0))
            {
                //Finalise scrolling
                itemSelectionValid = false;
                BackPackManager.Instance.FinaliseScrolling();
                inputState = InputState.waitingForInput;
            }
        }
        else if (inputState == InputState.moving)
        {
            MoveItem();

            if (Input.GetMouseButtonUp(0))		
                DropItem();
        }
        else if (inputState == InputState.rotating)
        {
            RotateItem();

            if (Input.GetMouseButtonUp(0))
                FinaliseRotation();
        }
    }
    //Touch controls
    void TouchControls()
    {
        foreach (Touch touch in Input.touches)
        {
            //Get the position of the input
            inputPos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
            inputPos.z = 0;

            //Cast a ray to detect objets
            hit = Physics2D.Raycast(inputPos, new Vector2(0, 0), 0.1f, mask);

            if (inputState == InputState.waitingForInput)
            {
                ScanForInput();
            }
            else if (inputState == InputState.scrolling)
            {
                ScrollBackPack();

                //If the input was released
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    //Finalise scrolling
                    itemSelectionValid = false;
                    BackPackManager.Instance.FinaliseScrolling();
                    inputState = InputState.waitingForInput;
                }
            }
            else if (inputState == InputState.moving)
            {
                MoveItem();

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    DropItem();
            }
            else if (inputState == InputState.rotating)
            {
                RotateItem();

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    FinaliseRotation();
            }
        }
    }

    //Called when there are no specific input and waiting for one
    void ScanForInput()
    {
        if (HasInput())
        {
            //If the input was registered
            if (hit.collider != null)
            {
                if (hit.transform.tag == "GUI")
                {
                    if (hasFeedback)
                        HideFeedback();

                    GUIManager.Instance.ReceiveInput(hit.transform);
                }

                else if (hit.transform.tag == "BackPack")
                    PrepareScrolling(hit.transform);

                else if (hit.transform.tag == "GameObject")
                    PrepareToDrag(hit.transform, false);

                else if (hit.transform.tag == "Feedback")
                    PrepareToRotate();


            }
            //If we have an active feedback, and the input was in an empty space, hide the feedback
            else if (hasFeedback)
			{
				selectedObject = null;
                HideFeedback();
			}
        }
    }

	public void ProcessSelectedObject(Transform t)
	{
		PrepareToDrag (t, true);
	}

	//Prepare the BackPack for scrolling
    void PrepareScrolling(Transform t)
    {
        //If the input was on a BackPack item, register it
        if (hit.transform.name != "background" && hit.transform.name != "backgroundCap")
        {
            selectedItem = hit.transform;
            itemSelectionValid = true;
        }

        //Prepare for scrolling
        BackPackManager.Instance.PrepareScolling();
        scrollStartingPos = inputPos.x;
        inputState = InputState.scrolling;
    }
    //Scrolls BackPack content
    void ScrollBackPack()
    {
        //If the input is on a BackPack item
        if (itemSelectionValid)
        {
            //If we moved the input up while the distance to the starting input is not greater then 0.5
            if (Mathf.Abs(inputPos.x - scrollStartingPos) < 0.5f && inputPos.y > -2.8f)
            {
                //Prepare the item for dragging
                PrepareToDrag(selectedItem, true);
            }
            //If the distance to the starting input is greater than 0.5
            else if (Mathf.Abs(inputPos.x - scrollStartingPos) > 0.5f)
            {
                //Then the item is no longer valid for dragging
                itemSelectionValid = false;

                //Set the current position for the scroll starting position
                scrollStartingPos = inputPos.x;
            }
        }
        else
        {
            //Scroll the BackPack
            BackPackManager.Instance.ScrollMode(inputPos.x - scrollStartingPos);
        }
    }

    //Prepares the selected item for dragging
    void PrepareToDrag(Transform item, bool fromBackPack)
    {
        //If the level is in play mode, return to caller
        if (LevelDesignManager.Instance.InPlayMode())
            return;

        //If we have an active feedback, hide it
        if (hasFeedback && selectedObject != null)
            HideFeedback();


        //If the object is from the BackPack
        if (fromBackPack)
        {
			fromBackPack = true;
			//Remove the object from the BackPack, and select it
			selectedObject = item.GetComponent<BackPackItemType>().RemoveItem();
            selectedObject.DragMode();
            selectedObject.PlayPickupAnimation();

            //Activate the feedback on the object
            TransformObjectManager.Instance.Setup(selectedObject, TransformObjectManager.TargetState.dragging);
            hasFeedback = true;

            //Render the object to the top 
            ChangeSortingOrderBy(selectedObject.gameObject, 3);
            inputState = InputState.moving;

        }
        //If the object is not from the BackPack, make sure it can be dragged
        else if (CanDragged(item))
        {
			fromBackPack = false;
            //It is possible we have selected its child collider, so scan it for the ObjectBase script
            selectedObject = GetParent(item);
            
            selectedObject.DragMode();

	
            //Calculate offset based on input position
            offset = new Vector3(inputPos.x - selectedObject.transform.position.x, inputPos.y - selectedObject.transform.position.y, 0);

            //Activate feedback on the selected item
            TransformObjectManager.Instance.Setup(selectedObject, TransformObjectManager.TargetState.dragging);
            hasFeedback = true;

            //Render the object to the top 
            ChangeSortingOrderBy(selectedObject.gameObject, 3);
            inputState = InputState.moving;
			lastPosition = selectedObject.transform.position;
			if ( selectedObject != null )
			{
				pItemSlected = true;
			}
        }
        //if item already existing item which cant be dragged 
        else
        {
			selectedObject = null;
        }
    }
    //Moves the selected item 
    void MoveItem()
    {
		Rect ScreenRect = new Rect (0,0, Screen.width, Screen.height);
        //Move the item to the input position based on the starting offset
		if(ScreenRect.Contains(Input.mousePosition))
        	selectedObject.transform.position = new Vector3(inputPos.x - offset.x, inputPos.y - offset.y , 0);
    }

    //Drops the selected item
    void DropItem()
    {
        //Render the object on it's original order
        ChangeSortingOrderBy(selectedObject.gameObject, -3);

		// set feedback to rotation as soon as object is selected and on mouse button up
		TransformObjectManager.Instance.Setup(selectedObject, TransformObjectManager.TargetState.rotating);
		pItemSlected = false;
        //If the object is in a valid position
        if (selectedObject.GetValidPos())
        {
            //Drop it and add it to the active items
            selectedObject.Dropped();
            selectedObject.Setup();
            LevelDesignManager.Instance.AddItem(selectedObject.GetComponent<ObjectBase>());

            //If the object can be rotated, change the feedback to rotation
            if (selectedObject.canRotate)
			{
                TransformObjectManager.Instance.Setup(selectedObject, TransformObjectManager.TargetState.rotating);
			}
            else
			{
				TransformObjectManager.Instance.Disable(0);
				HideFeedback();
                selectedObject = null;
			}
        }
		else
		{
			//Puts it to previous valid position if dropped to a invalid position
			//if placed from BackPack to a invalid position put it back to BackPack
			if( fromBackPack ==false)
			{
				//the object is dropped in invalid position
				selectedObject.transform.position = lastPosition;
				selectedObject.SetValidPos(true);
			}
			else if ( fromBackPack == true)
			{
	            //Put the item back to the BackPack 
	            BackPackManager.Instance.AddItem(selectedObject);
				LevelDesignManager.Instance.RemoveItem(selectedObject.GetComponent<ObjectBase>());
	            TransformObjectManager.Instance.Disable(0);

	            selectedObject = null;
				HideFeedback();
	        }
		}

        //Reset item variables

        offset = Vector3.zero;
        selectedItem = null;
        itemSelectionValid = false;

        //Set input state
        inputState = InputState.waitingForInput;
    }

    //Prepares the item for rotating
    void PrepareToRotate()
    {
        //If the level is in play mode, return to caller
        if (LevelDesignManager.Instance.InPlayMode())
            return;

        //If we clicked on the feedback, while it is not in rotation mode, return to caller
        if (!TransformObjectManager.Instance.InRotation())
            return;

        //Put the selected object into drag mode, and calculate starting vector
        selectedObject.DragMode();
        startingVector = new Vector2(inputPos.x - selectedObject.transform.position.x, inputPos.y - selectedObject.transform.position.y);
        startingVector.Normalize();

        startingRot = selectedObject.transform.eulerAngles;

        //Make the feedback to rotate with the selected object
        TransformObjectManager.Instance.RotateWith(selectedObject.transform);

        //Render the object on the top
        ChangeSortingOrderBy(selectedObject.gameObject, 3);
        inputState = InputState.rotating;
    }
    //Rotates the selected item
    void RotateItem()
    {
        //Calculate current rotation vector
        Vector3 currentVector = new Vector2(inputPos.x - selectedObject.transform.position.x, inputPos.y - selectedObject.transform.position.y);
        currentVector.Normalize();

        //Get the current rotation
        Vector3 currentRotation = selectedObject.transform.eulerAngles;

        //Calculate the angle between the starting and current vector
        float angle = Vector3.Angle(startingVector, currentVector);

        //Calculate a middle vector between the starting and current vector, and caclulate rotation based on it
        if (Vector3.Cross(startingVector, currentVector).z < 0)
            currentRotation.z = startingRot.z - angle;
        else
            currentRotation.z = startingRot.z + angle;

        //Apply rotation
        selectedObject.transform.eulerAngles = currentRotation;
    }
    //Finalise rotation for the current input
    void FinaliseRotation()
    {
        //Render the object on it's original order
        ChangeSortingOrderBy(selectedObject.gameObject, -3);

        //Make the feedback rotate independently
        TransformObjectManager.Instance.RotateAlone();

        //Stop rotation and add the item to the active items
        selectedObject.RotationEnded();
        LevelDesignManager.Instance.AddItem(selectedObject.GetComponent<ObjectBase>());

        //Reset item variables
        offset = Vector3.zero;
        selectedItem = null;
        itemSelectionValid = false;

        inputState = InputState.waitingForInput;
    }

    //Hides active feedback
    void HideFeedback()
    {
        TransformObjectManager.Instance.Disable(0.2f);
        hasFeedback = false;
    }
    //Returns true if there is an active input
    bool HasInput()
    {
        if (useTouch)
            return Input.touchCount > 0;
        else
            return Input.GetMouseButtonDown(0);
    }
    
    //Returns true, if the object can be dragged around by the player
    bool CanDragged(Transform item)
    {
        while (item != null)
        {
            if (item.name.Contains("(Clone)"))
                return true;

            item = item.parent;
        }

        return false;
    }
    //Change the sorting layer of obj and its children
    void ChangeSortingOrderBy(GameObject obj, int by)
    {
        if (obj.GetComponent<SpriteRenderer>())
            obj.GetComponent<SpriteRenderer>().sortingOrder += by;

        foreach(Transform child in obj.transform)
            ChangeSortingOrderBy(child.gameObject, by);
    }
    //Returns the ObjectBase parent of the item
    ObjectBase GetParent(Transform item)
    {
        while (item.GetComponent<ObjectBase>() == null)
        {
            item = item.parent;
        }

        return item.GetComponent<ObjectBase>();
    }

	public void RemoveSelectedLevelItem()
	{
		if(selectedObject != null && selectedObject.GetValidPos())
		{
			//Put the item back to the BackPack
			BackPackManager.Instance.AddItem(selectedObject);
			LevelDesignManager.Instance.RemoveItem(selectedObject.GetComponent<ObjectBase>());
			TransformObjectManager.Instance.Disable(0);
			
			selectedObject = null;
		}

		//Reset item variables
		offset = Vector3.zero;
		selectedItem = null;
		itemSelectionValid = false;

		pItemSlected = false;
			
		//Set input states
		inputState = InputState.waitingForInput;

		if(hasFeedback)
			HideFeedback();

    }

	public bool IsObjectSelected()
	{
		return (selectedObject != null);
	}
}
