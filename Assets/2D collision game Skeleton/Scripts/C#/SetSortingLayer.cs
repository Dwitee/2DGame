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

public class SetSortingLayer : MonoBehaviour 
{
    public string layerName = "GameObject";
    public int sortingOrder = 5;

	void Start () 
    {
        this.renderer.sortingLayerName = layerName;
        this.renderer.sortingOrder = sortingOrder;
	}
}
