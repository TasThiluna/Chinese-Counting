using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
//using Rnd = UnityEngine.Random;

public class chineseCounting : MonoBehaviour
{
    public KMAudio audio;
    public KMBombInfo bomb;
    public KMSelectable[] keys;
    public TextMesh[] keysText;

    public Material[] ledColors;
    public Material[] led2Colors;
    public Renderer led;
    public Renderer led2;
    private int ledIndex = 0;
    private int led2Index = 0;

    public String[] ascending;
    public String[] descending;
    public String[] charascending;
    public String[] chardescending;

    private String[] correctOrder = new String[20];
    private List <int> pickedNumbersIndex;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable key in keys)
        {
          KMSelectable pressedKey = key;
          key.OnInteract += delegate () { keysPress(pressedKey); return false; };
        }
    }

    void Start()
    {
        PickLEDColors();
        DetermineList();
        PickNumbers();
    }

    void PickLEDColors()
    {
      ledIndex = UnityEngine.Random.Range(0,4);
      led2Index = UnityEngine.Random.Range(0,4);
      led.material = ledColors[ledIndex];
      led2.material = led2Colors[led2Index];
      Debug.LogFormat("[Chinese Couting #{0}] The left LED is {1}, and the right LED is {2}.", moduleId, ledColors[ledIndex].name, led2Colors[led2Index].name);
    }

    void DetermineList()
    {
      if((ledIndex == 1 && led2Index == 1) || (ledIndex == 2 && led2Index == 3) || (ledIndex == 3 && led2Index == 4) || (ledIndex == 4 && led2Index == 2))
      {
        for(int i = 0; i <= 29; i++)
        {
          correctOrder[i] = ascending[i];
          Debug.LogFormat("[Chinese Coutning #{0}] The numbers should be pressed in asending order, by value.", moduleId);
        }
      }
      else if((ledIndex == 1 && led2Index == 2) || (ledIndex == 2 && ledIndex == 2) || (ledIndex == 3 && led2Index == 3) || (ledIndex == 4 && led2Index == 4))
      {
        for(int i = 0; i <= 29; i++)
        {
          correctOrder[i] = descending[i];
          Debug.LogFormat("[Chinese Coutning #{0}] The numbers should be pressed in descending order, by value.", moduleId);
        }
      }
      else if((ledIndex == 1 && led2Index == 2) || (ledIndex == 2 && led2Index == 4) || (ledIndex == 3 && led2Index == 1) || (ledIndex == 4 && led2Index == 2))
      {
        for(int i = 0; i <= 29; i++)
        {
          correctOrder[i] = charascending[i];
          Debug.LogFormat("[Chinese Coutning #{0}] The numbers should be pressed in descending order, by number of characters.", moduleId);
        }
      }
      else
      {
        for(int i = 0; i <= 29; i++)
        {
          correctOrder[i] = chardescending[i];
          Debug.LogFormat("[Chinese Coutning #{0}] The numbers should be pressed in descending order, by number of characters.", moduleId);
        }
      }

    }

    void PickNumbers()
    {
      for(int i = 0; i <= 3; i++)
      {
        int index = UnityEngine.Random.Range(0,30);
        while(pickedNumbersIndex.Contains(index))
        {
          index = UnityEngine.Random.Range(0,30);
        }
        pickedNumbersIndex.Add(index);

      }
    }

    void keysPress(KMSelectable key)
    {
      Debug.LogFormat("[Chinese Counting #{0}] You pressed {1}.", moduleId, key.GetComponentInChildren<TextMesh>().text);
    }

}
