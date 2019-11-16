using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceDisplay : MonoBehaviour
{

    [SerializeField]
    public CityPlayer player;

    [SerializeField]
    public GameObject textObj;

    private GameObject[] texts;

    // Start is called before the first frame update
    void Start()
    {
        texts = new GameObject[Enum.GetNames(typeof(ResourceTypeEnum)).Length - 1]; // Lower length because we don't use NONE

        for(int i = 0; i < texts.Length; i++)
        {
            GameObject t = Instantiate(textObj);
            ResourceTypeEnum rte = (ResourceTypeEnum) (i+1); // Gets the (i+1)th ResourceTypeEnum
            t.GetComponent<Text>().text = rte + ": 0";
            texts[i] = t;
            t.transform.SetParent(player.uicanvas.transform, false);
            t.transform.Translate(-Screen.width/3 - 100 + i * 100, Screen.height/3 + 75, 0);  // TODO: find a better way to poition UI elements
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < texts.Length; i++)
        {
            GameObject t = texts[i];
            ResourceTypeEnum rte = (ResourceTypeEnum) (i+1);
            t.GetComponent<Text>().text = rte + ": " + (int) player.resources[rte];
        }
    }
}
