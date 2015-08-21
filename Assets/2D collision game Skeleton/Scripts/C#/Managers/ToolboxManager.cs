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

public class BackPackManager : MonoBehaviour 
{
    enum BackPackState { open, close, inTransit };           //The possible states for the BackPack
    BackPackState backPackState = BackPackState.close;         //The current state of the BackPack

    public float maxBackPackMovement;                        //The maximum BackPack number
    public float spaceBetweenItems = 0;                     //The space between the items in the BackPack
    public Transform button;                                //The BackPack button
    public Transform strip;                                 //The BackPack strip
    public Transform itemContainer;                         //The main container for the BackPack items
    public Transform tempContainer;                         //The temp container for item movement

    static BackPackManager myInstance;                       //Holds a reference to this script

    List<BackPackItemType> items;                            //Holds the item types in the BackPack
    List<BackPackItemType> inactiveItems;                    //Holds the inactive item types

    float totalWidth;                                       //The total width of the BackPack
    float BackPackMovement;                                  //The currently allowed BackPack movement
    float scrollAmmount;                                    //The currently allowed scrolling
    float itemContainerStartPos;                            //The starting position of the container at the beginning of the scroll

    bool transitInProgress = false;                         //BackPack transit in progress/not in progress
    Vector3 itemContainerPos;                               //The position of the container when scrolling

    //Returns the instance
    public static BackPackManager Instance { get { return myInstance; } }

    //Called at the beginning of the level
    void Start()
    {
        myInstance = this;
        button.GetComponent<SpriteRenderer>().sortingLayerName = "GUI";
        itemContainerPos = itemContainer.transform.position;

        items = new List<BackPackItemType>();
        inactiveItems = new List<BackPackItemType>();
    }

    //Add the items for the container and arrange them
    public void SetupToolbar(List<BackPackItemType> toolbarItems)
    {
        totalWidth = 0;
        float offset = 1; //move the items away from Screen 
        float lastWidth = 0;
        float currentWidth = 0;
		float lastPos = button.transform.position.x + offset; //As item is active it is clikable adding offset to avoid it
        Vector3 newPos;

        //Loops through the received items
        for (int i = 0; i < toolbarItems.Count; i++)
        {
            //Gets the current item width and calculates the next position
            currentWidth = toolbarItems[i].GetWidth();
            newPos = new Vector3(lastPos + (lastWidth / 2) + spaceBetweenItems + (currentWidth / 2), button.transform.position.y, 0);

            //Set the item to its position
            toolbarItems[i].transform.parent = itemContainer;
            toolbarItems[i].SetPosition(newPos);
            items.Add(toolbarItems[i]);

            lastWidth = currentWidth;
            lastPos = newPos.x;
        }

        //Calculate BackPack movement variable
        totalWidth = lastPos - button.transform.position.x + (lastWidth / 2) + (4 * spaceBetweenItems);
        BackPackMovement = totalWidth;
        scrollAmmount = 0;

        if (BackPackMovement > maxBackPackMovement)
        {
            BackPackMovement = maxBackPackMovement;
            scrollAmmount = totalWidth - BackPackMovement;
        }
    }

    //Called when an input is registered on the BackPack button
    public void ButtonPressed()
    {
        if (backPackState == BackPackState.close && items.Count > 0)
            StartCoroutine(MoveHorizontalBy(strip.transform, -BackPackMovement, 0.2f, true));
        else if (backPackState == BackPackState.open)
            StartCoroutine(MoveHorizontalBy(strip.transform, BackPackMovement, 0.2f, true));
    }
    //Called by the GUI manager, shows the BackPack
    public void ShowBackPack()
    {
        StartCoroutine("Show");
    }
    //Called by the GUI manager, hides the BackPack
    public void HideBackPack()
    {
        StartCoroutine("Hide");
    }

    //Prepare the BackPack for scrolling
    public void PrepareScolling()
    {
        itemContainerStartPos = itemContainer.position.x;
    }
    //Scrolls the BackPack
    public void ScrollMode(float value)
    {
        itemContainerPos.x = itemContainerStartPos + value;
        itemContainer.position = itemContainerPos;
    }
    //Finalise BackPack scrolling
    public void FinaliseScrolling()
    {
        float moveBy = 0;
        
        //If the BackPack is scrolled out of its zone, move it back
        if (itemContainer.localPosition.x > 0)
            moveBy = -itemContainer.localPosition.x;
        else if (itemContainer.localPosition.x  < -scrollAmmount)
            moveBy = -(itemContainer.localPosition.x + scrollAmmount);

        if (moveBy != 0)
            StartCoroutine(MoveHorizontalBy(itemContainer, moveBy, 0.2f, false));
    }

    //Put an item to the BackPack
    public void AddItem(ObjectBase item)
    {
        //Get the name of the item
        string itemName = item.name.Substring(0, item.name.IndexOf('('));
        
        //If its BackPack counterpart is active, add the item to it
        foreach (BackPackItemType itemType in items)
        {
            if (itemType.ContentName() == itemName)
            {
                itemType.AddItem(item);
                return;
            }
        }
        
        //If we did not find the counterpart in the active objects, scan the inactive items
        foreach (BackPackItemType itemType in inactiveItems)
        {
            //If we found it, activate it
            if (itemType.ContentName() == itemName)
            {
                StartCoroutine(AddItemType(itemType, item));
                return;
            }
        }
    }
    //Removes every element from the BackPack
    public void ClearBackPack()
    {
        //Resets the BackPack elements
        while (items.Count > 0)
        {
            items[0].Reset();
            items.RemoveAt(0);
        }

        while (inactiveItems.Count > 0)
        {
            inactiveItems[0].gameObject.SetActive(true);
            inactiveItems[0].Reset();

            inactiveItems.RemoveAt(0);
        }

        //Reset the variables
        totalWidth = 0;
        BackPackMovement = 0;
        scrollAmmount = 0;
    }
    //Disables and removes an item type from the BackPack
    public void DisableItemType(BackPackItemType item)
    {
        //If this is the last item in the BackPack, simply remove it
        if (items.Count == 1)
        {
            inactiveItems.Add(items[0]);

           totalWidth = 0;
            BackPackMovement = 0;
            scrollAmmount = 0;

            items[0].gameObject.SetActive(false);

            items.Remove(items[0]);
            Close();
        }
        //If there is more than one item left in the BackPack, "remove" animation is played
        else
            StartCoroutine("RemoveItemType", item);
    }

    //Opens the BackPack
    void Open()
    {
        if (backPackState == BackPackState.close && items.Count > 0)
            StartCoroutine(MoveHorizontalBy(strip.transform, -BackPackMovement, 0.2f, true));
    }
    //Closes the BackPack
    void Close()
    {
        if (backPackState == BackPackState.open)
        {
            if (items.Count == 0)
                StartCoroutine(MoveHorizontalBy(strip.transform, -strip.transform.localPosition.x, 0.2f, true));
            else
                StartCoroutine(MoveHorizontalBy(strip.transform, BackPackMovement, 0.2f, true));
        }
    }
    //Returns the index of t in the items array
    int GetIndex(BackPackItemType t)
    {
        int i = 0;
        while (i < items.Count && t.ContentName() != items[i].ContentName())
            i++;

        return i;
    }
    //Recalculated BackPack movement related variables based
    void ModifyBackPackValues(float addToWidth)
    {
        totalWidth += addToWidth;

        if (totalWidth > maxBackPackMovement)
        {
            BackPackMovement = maxBackPackMovement;
            scrollAmmount = totalWidth - BackPackMovement;
        }
        else
        {
            scrollAmmount = 0;

            if (items.Count > 0)
            {
                BackPackMovement = totalWidth;
            }
            else
            {
                BackPackMovement = 0;
                totalWidth = 0;
            }
        }
    }   
    //Makes the items a child of the item container
    void ResetParents()
    {
        foreach (BackPackItemType item in items)
            item.transform.parent = itemContainer;
    }
    //Makes the content of the received list a child of the temp container
    void MoveToTemp(List<BackPackItemType> l)
    {
        foreach (BackPackItemType item in l)
            item.transform.parent = tempContainer;
    }
    //Returns the items between items[index+1] and items[items.Count]
    List<BackPackItemType> GetItemsAfter(int index)
    {
        List<BackPackItemType> rightItems = new List<BackPackItemType>();

        for (int i = index+1; i < items.Count; i++)
            rightItems.Add(items[i]);

        return rightItems;
    }

    //Adds an item type to the BackPack
    IEnumerator AddItemType(BackPackItemType t, ObjectBase item)
    {
        t.AddItem(item);
        
        while (transitInProgress)
            yield return new WaitForEndOfFrame();

        transitInProgress = true;

        float newTypePos;
        float addWidth;

        //If the BackPack is empty, calculate new position from the BackPack button
        if (items.Count == 0)
        {
            newTypePos = button.transform.position.x + (t.GetWidth() / 2) + (spaceBetweenItems);
            addWidth = t.GetWidth() + (spaceBetweenItems * 5);
        }
        //Else, calculate new position from the last item
        else
        {
            newTypePos = items[items.Count - 1].transform.position.x + (items[items.Count - 1].GetWidth() / 2) + spaceBetweenItems + (t.GetWidth() / 2);
            addWidth = t.GetWidth() + spaceBetweenItems;
        }

        Vector3 newPos = new Vector3(newTypePos, button.transform.position.y, t.transform.position.z);

        //Reset the type 
        t.SetPosition(newPos);
        t.gameObject.SetActive(true);

        //Remove the type from the inactive list and add it to the active list
        inactiveItems.Remove(t);
        items.Add(t);

        ModifyBackPackValues(addWidth);

        yield return new WaitForEndOfFrame();

        //If the newly added item is the only item, open the BackPack
        if (items.Count == 1)
        {
            Open();
            yield return new WaitForSeconds(0.2f);
        }
        //If the item can fit into the BackPack without scrolling, extend the BackPack
        else if (scrollAmmount == 0)
        {
            StartCoroutine(MoveHorizontalBy(strip, -(t.GetWidth() + spaceBetweenItems), 0.2f, false));
            yield return new WaitForSeconds(0.2f);
        }

        transitInProgress = false;
    }
    //Removes an item type from the BackPack
    IEnumerator RemoveItemType(BackPackItemType t)
    {
        while (transitInProgress)
            yield return new WaitForEndOfFrame();

        transitInProgress = true;

        //Get the index of the item and store the items to its right
        int index = GetIndex(t);
        List<BackPackItemType> right = GetItemsAfter(index);

        //Remove the item type from the items list
        items.Remove(t);
        inactiveItems.Add(t);
        t.gameObject.SetActive(false);

        //Place the right items to the temp, and move them to their new place
        MoveToTemp(right);
        StartCoroutine(MoveHorizontalBy(tempContainer, -(t.GetWidth() + spaceBetweenItems), 0.2f, false));

        //Modify BackPack movement variables
        ModifyBackPackValues(-(t.GetWidth() + spaceBetweenItems));

        //StartCoroutine("Test");
        yield return new WaitForSeconds(0.2f);

        //Copy back the items from the temp to their original position
        ResetParents();

        //If the items can fit into the whole BackPack
        if (totalWidth < maxBackPackMovement && items.Count > 0)
        {
            //Move the BackPack to the right
            float movementAmmount = button.transform.position.x - (3 * spaceBetweenItems) - (items[items.Count - 1].GetWidth() / 2);
            movementAmmount -= items[items.Count - 1].transform.position.x;

            StartCoroutine(MoveHorizontalBy(strip, movementAmmount, 0.2f, false));
            yield return new WaitForSeconds(0.2f);
        }

        //Finalise item scrolling
        FinaliseScrolling();

        transitInProgress = false;
    }
    //Hides the BackPack
    IEnumerator Hide()
    {
        //If the BackPack is open
        if (backPackState == BackPackState.open)
        {
            //Close it and wait for it
            StartCoroutine(MoveHorizontalBy(strip, BackPackMovement, 0.4f, false));
            yield return new WaitForSeconds(0.4f);
            strip.transform.localPosition = new Vector2(0, strip.transform.localPosition.y);
        }

        //Hide the BackPack
        strip.gameObject.SetActive(false);
        StartCoroutine(MoveHorizontalBy(this.transform, 1.5f, 0.35f, false));
    }
    //Shows the BackPack
    IEnumerator Show()
    {
        //Show the BackPack and wait for it
        StartCoroutine(MoveHorizontalBy(this.transform, -1.5f, 0.35f, false));
        yield return new WaitForSeconds(0.35f);

        //Reactivate BackPack strip
        strip.gameObject.SetActive(true);

        //If the BackPack state is open
        if (backPackState == BackPackState.open)
        {
            //Open it
            StartCoroutine(MoveHorizontalBy(strip, -BackPackMovement, 0.4f, true));
        }
    }
    //Moves an object by an ammount, under time
    IEnumerator MoveHorizontalBy(Transform obj, float moveBy, float time, bool changeBackPackState)
    {
        //If changeing state is allowed, change to in transit state
        if (changeBackPackState)
            backPackState = BackPackState.inTransit;

        //Move the menu to the designated position under time
        float i = 0.0f;
        float rate = 1.0f / time;

        Vector3 startPos = obj.localPosition;
        Vector3 endPos = startPos;
        endPos.x += moveBy;

        while (i < 1.0)
        {
            i += Time.deltaTime * rate;
            obj.localPosition = Vector3.Lerp(startPos, endPos, i);
            yield return 0;
        }

        //If changing state is allowed, change to the right state
        if (changeBackPackState)
        {
            if (moveBy < 0)
                backPackState = BackPackState.open;
            else
                backPackState = BackPackState.close;
        }
    }
}
