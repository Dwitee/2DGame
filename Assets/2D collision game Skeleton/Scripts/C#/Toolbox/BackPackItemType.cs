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

public class BackPackItemType : MonoBehaviour
{
    public int count;                           //The number of items the BackPack holds
    public float width;                         //The width of the BackPack in units (100 pixel = 1 unit)
    public ObjectBase normalPrefab;             //A link to the prefab the item holds
    public SpriteRenderer counter;              //A link to the counter
    public Sprite[] BackPackNumbers;             //Holds the BackPack numbers

    public List<ObjectBase> content;                   //The content the item holds
    public List<ObjectBase> placedContent;             //The content the item had, but now are placed

    //Called at the beginning of the levels
    public void Start()
    {
        //Create the lists
        content = new List<ObjectBase>();
        placedContent = new List<ObjectBase>();
        
        //If the item in the BackPack is available more than once, enable the counter, and set its number
        if (count > 1)
        {
            counter.gameObject.SetActive(true);
            counter.sprite = BackPackNumbers[count - 2];
        }

        //Spawn the required number of clones from the prefab
        for (int i = 0; i < count; i++)
        {
            ObjectBase go = (ObjectBase)Instantiate(normalPrefab, transform.position, Quaternion.identity);
            content.Add(go.GetComponent<ObjectBase>());

            go.transform.parent = this.transform;
            go.gameObject.SetActive(false);
        }
    }

    //Updates the position of the item
    public void SetPosition (Vector3 pos)
    {
        this.transform.position = pos;
    }
    //Adds a prefab clone to the item, and updates counter
    public void AddItem(ObjectBase item)
    {
        placedContent.Remove(item);
        content.Add(item);

        item.transform.eulerAngles = Vector3.zero;
        item.gameObject.SetActive(false);

        if (count == 1)
            counter.gameObject.SetActive(true);

        count++;

        if (count > 1)
            counter.sprite = BackPackNumbers[count - 2];
    }

    //Removes and returns the first prefab clone from the content
    public ObjectBase RemoveItem()
    {
        ObjectBase go = content[0];

        placedContent.Add(go);
        content.Remove(go);

        go.transform.position = new Vector3(0, -100, 0);
        go.gameObject.SetActive(true);

        count--;
        if (count > 1)
            counter.sprite = BackPackNumbers[count - 2];
        else if (count == 1)
            counter.gameObject.SetActive(false);
        else
            BackPackManager.Instance.DisableItemType(this);

        return go;
    }

    //Resets the item
    public void Reset()
    {
        //Removes the placed elements from the map
        while (placedContent.Count > 0)
        {
            LevelDesignManager.Instance.RemoveItem(placedContent[0]);
            AddItem(placedContent[0]);        
        }

        //Update counter
        if (count > 2)
            counter.sprite = BackPackNumbers[count - 2];
    }
    //Returns the width of the item in units
    public float GetWidth()
    {
        return width;
    }
    //Returns the name of the prefab
    public string ContentName()
    {
        return normalPrefab.name;
    }
}
