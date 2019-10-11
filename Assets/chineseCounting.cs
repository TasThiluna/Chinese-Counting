using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
//using Rnd = UnityEngine.Random;

public class chineseCounting : MonoBehaviour
{
    public KMAudio Audio;
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

    private String Solution = "";

    private String[] correctOrder = new String[30];
    private List <int> pickedNumbersIndex = new List <int>();
    private List <int> pickedNumbersIndexOrdered = new List <int>();

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private int stage = 0;
    private bool wrong;

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
        PickOrder();
    }

    void PickLEDColors()
    {
      ledIndex = UnityEngine.Random.Range(0,4);
      led2Index = UnityEngine.Random.Range(0,4);
      led.material = ledColors[ledIndex];
      led2.material = led2Colors[led2Index];
      Debug.LogFormat("[Chinese Counting #{0}] The left LED is {1}, and the right LED is {2}.", moduleId, ledColors[ledIndex].name, led2Colors[led2Index].name);
    }

    void DetermineList()
    {
      if((ledIndex == 0 && led2Index == 0) || (ledIndex == 1 && led2Index == 2) || (ledIndex == 2 && led2Index == 3) || (ledIndex == 3 && led2Index == 1))
      {
        for(int i = 0; i <= 29; i++)
        {
          correctOrder[i] = ascending[i];
        }
        Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in asending order, by value.", moduleId);
      }
      else if((ledIndex == 0 && led2Index == 3) || (ledIndex == 1 && ledIndex == 1) || (ledIndex == 2 && led2Index == 2) || (ledIndex == 3 && led2Index == 3))
      {
        for(int i = 0; i <= 29; i++)
        {
          correctOrder[i] = descending[i];
        }
          Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in descending order, by value.", moduleId);
      }
      else if((ledIndex == 0 && led2Index == 1) || (ledIndex == 1 && led2Index == 3) || (ledIndex == 2 && led2Index == 0) || (ledIndex == 3 && led2Index == 2))
      {
        for(int i = 0; i <= 29; i++)
        {
          correctOrder[i] = charascending[i];
        }
        Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in descending order, by number of characters.", moduleId);
      }
      else
      {
        for(int i = 0; i <= 29; i++)
        {
          correctOrder[i] = chardescending[i];
        }
        Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in descending order, by number of characters.", moduleId);
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
        keysText[i].text = correctOrder[index];
      }
      Debug.LogFormat("[Chinese Counting #{0} The numbers are: {1}, {2}, {3}, and {4}.", moduleId, correctOrder[pickedNumbersIndex[0]], correctOrder[pickedNumbersIndex[1]], correctOrder[pickedNumbersIndex[2]], correctOrder[pickedNumbersIndex[3]]);
    }

    void PickOrder()
    {
      pickedNumbersIndexOrdered = pickedNumbersIndex.ToList();
      pickedNumbersIndexOrdered.Sort();
      Debug.LogFormat("[Chinese Counting #{0}] The keys should be pressed in this order: {1}, {2}, {3}, and then {4}.", moduleId, correctOrder[pickedNumbersIndexOrdered[0]], correctOrder[pickedNumbersIndexOrdered[1]], correctOrder[pickedNumbersIndexOrdered[2]], correctOrder[pickedNumbersIndexOrdered[3]]);
    }

    void keysPress(KMSelectable key)
    {
      if (moduleSolved)
      {
        return;
      }
      Solution = correctOrder[pickedNumbersIndexOrdered[stage]];
      Debug.LogFormat("[Chinese Counting #{0}] You pressed {1}.", moduleId, key.GetComponentInChildren<TextMesh>().text);
      if(key.GetComponentInChildren<TextMesh>().text != Solution)
      {
        wrong = true;
      }
      stage++;
      if(stage == 4)
      {
        stage = 0;
        if(!wrong)
        {
          moduleSolved = true;
          GetComponent<KMBombModule>().HandlePass();
          Debug.LogFormat("[Chinese Counting #{0}] Module solved.", moduleId);
        }
        else
        {
          GetComponent<KMBombModule>().HandleStrike();
          Debug.LogFormat("[Chinese Counting #{0}] Strike! Resetting...", moduleId);
          pickedNumbersIndex.Clear();
          pickedNumbersIndexOrdered.Clear();
        }
      }
    }

}
