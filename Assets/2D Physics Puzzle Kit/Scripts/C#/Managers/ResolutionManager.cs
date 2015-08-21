
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

public class ScreenResolutionManager : MonoBehaviour 
{
    public Transform[] toReposition;
    public Transform BackPack;

    public float scale5x4;                          //The level scale for 5:4 aspect ratio 
    public float scale4x3;                          //The level scale for 4:3 aspect ratio 
    public float scale3x2;                          //The level scale for 3:2 aspect ratio 
    public float scale16x10;                        //The level scale for 16:10 aspect ratio 
    public float scale16x9;                         //The level scale for 16:9 aspect ratio 
    public float scale5x3;                          //The level scale for 5:3 aspect ratio 

    private float scaleFactor;                      //The current scale factor

    // Use this for initialization
    void Start()
    {
        CalculateAspectRatio();

        foreach (Transform item in toReposition)
		{
			if(item != null)
            	item.position = new Vector3(item.position.x * scaleFactor, item.position.y, item.position.z); 
		}

		BackPack.position = new Vector3((scaleFactor - 1) * 4, BackPack.position.y, BackPack.position.z);
    }
    //Calculates the current aspect ratio
    private void CalculateAspectRatio()
    {
        //Aspect Ratio: 5:4
        if (Mathf.Abs(Camera.main.aspect - 1.25f) < 0.01f)
        {
            //print("5:4");
            scaleFactor = scale5x4;
        }
        //Aspect Ratio: 4:3
        else if (Mathf.Abs(Camera.main.aspect - 1.33f) < 0.01f)
        {
            //print("4:3");
            scaleFactor = scale4x3;
        }
        //Aspect Ratio: 3:2
        else if (Mathf.Abs(Camera.main.aspect - 1.5f) < 0.01f)
        {
            //print("3:2");
            scaleFactor = scale3x2;
        }
        //Aspect Ratio: 16:10
        else if (Mathf.Abs(Camera.main.aspect - 1.6f) < 0.01f)
        {
            //print("16:10");
            scaleFactor = scale16x10;
        }
        //Aspect Ratio: 16:9
        else if (Mathf.Abs(Camera.main.aspect - 1.77f) < 0.01f)
        {
            //print("16:9");
            scaleFactor = scale16x9;
        }
        //Aspect Ratio: 5:3
        else if (Mathf.Abs(Camera.main.aspect - 1.66f) < 0.01f)
        {
            //print("5:3");
            scaleFactor = scale5x3;
        }
        else
        {
            print("Aspect Ratio not found");
            scaleFactor = 1;
        }
    }
}
