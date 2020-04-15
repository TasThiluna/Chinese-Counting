using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public Color[] numberColors;

    public String[] ascending;
    public String[] descending;
    public String[] charascending;
    public String[] chardescending;

    private String Solution = "";

    private String[] correctOrder = new String[30];
    private List<int> pickedNumbersIndex = new List<int>();
    private List<int> pickedNumbersIndexOrdered = new List<int>();
    private List<string> pressedKeys = new List<string>();

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private int stage = 0;
    private bool wrong;
    private bool recalcing;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable key in keys)
            key.OnInteract += delegate () { keysPress(key); return false; };
    }

    void Start()
    {
        recalcing = false;
        PickLEDColors();
        DetermineList();
        PickNumbers();
        PickOrder();
    }

    void PickLEDColors()
    {
        ledIndex = UnityEngine.Random.Range(0, 4);
        led2Index = UnityEngine.Random.Range(0, 4);
        led.material = ledColors[ledIndex];
        led2.material = led2Colors[led2Index];
        Debug.LogFormat("[Chinese Counting #{0}] The left LED is {1}, and the right LED is {2}.", moduleId, ledColors[ledIndex].name, led2Colors[led2Index].name);
    }

    void DetermineList()
    {
        var table = "ACHD,HDAC,CHDA,HACD".Split(',');
        switch (table[ledIndex][led2Index])
        {
            case 'A':
                correctOrder = ascending;
                Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in asending order, by value.", moduleId);
                break;

            case 'D':
                correctOrder = descending;
                Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in descending order, by value.", moduleId);
                break;

            case 'C':
                correctOrder = charascending;
                Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in ascending order, by number of characters.", moduleId);
                break;

            default:
                correctOrder = chardescending;
                Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in descending order, by number of characters.", moduleId);
                break;
        }
    }

    void PickNumbers()
    {
        for (int i = 0; i < 4; i++)
        {
            int index = UnityEngine.Random.Range(0, 30);
            while (pickedNumbersIndex.Contains(index))
                index = UnityEngine.Random.Range(0, 30);
            pickedNumbersIndex.Add(index);
            keysText[i].text = correctOrder[index];
            keysText[i].color = numberColors[0];
        }
        Debug.LogFormat("[Chinese Counting #{0}] The numbers are: {1}, {2}, {3}, and {4}.", moduleId, correctOrder[pickedNumbersIndex[0]], correctOrder[pickedNumbersIndex[1]], correctOrder[pickedNumbersIndex[2]], correctOrder[pickedNumbersIndex[3]]);
    }

    void PickOrder()
    {
        pickedNumbersIndexOrdered = pickedNumbersIndex.ToList();
        pickedNumbersIndexOrdered.Sort();
        Debug.LogFormat("[Chinese Counting #{0}] The keys should be pressed in this order: {1}, {2}, {3}, and then {4}.", moduleId, correctOrder[pickedNumbersIndexOrdered[0]], correctOrder[pickedNumbersIndexOrdered[1]], correctOrder[pickedNumbersIndexOrdered[2]], correctOrder[pickedNumbersIndexOrdered[3]]);
    }

    void keysPress(KMSelectable key)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, key.transform);
        key.AddInteractionPunch(.5f);
        if (moduleSolved || recalcing || pressedKeys.Contains(key.GetComponentInChildren<TextMesh>().text))
            return;
        pressedKeys.Add(key.GetComponentInChildren<TextMesh>().text);
        Solution = correctOrder[pickedNumbersIndexOrdered[stage]];
        Debug.LogFormat("[Chinese Counting #{0}] You pressed {1}.", moduleId, key.GetComponentInChildren<TextMesh>().text);
        if (key.GetComponentInChildren<TextMesh>().text != Solution)
            wrong = true;
        stage++;
        if (stage == 4)
        {
            stage = 0;
            if (!wrong)
            {
                moduleSolved = true;
                GetComponent<KMBombModule>().HandlePass();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                led.material = ledColors[4];
                led2.material = led2Colors[4];
                Debug.LogFormat("[Chinese Counting #{0}] Module solved.", moduleId);
            }
            else
            {
                wrong = false;
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Chinese Counting #{0}] Strike! Resetting...", moduleId);
                pickedNumbersIndex.Clear();
                pickedNumbersIndexOrdered.Clear();
                pressedKeys.Clear();
                StartCoroutine(Strike());
            }
        }
    }

    IEnumerator Strike()
    {
        led.material = ledColors[4];
        led2.material = led2Colors[4];
        recalcing = true;
        yield return new WaitForSeconds(0.5f);
        Start();
    }

    // Twitch Plays
    private bool cmdIsValid(string param)
    {
        string[] parameters = param.Split(' ', ',');
        for (int i = 1; i < parameters.Length; i++)
        {
            if (!parameters[i].EqualsIgnoreCase("1") && !parameters[i].EqualsIgnoreCase("2") && !parameters[i].EqualsIgnoreCase("3") && !parameters[i].EqualsIgnoreCase("4") && !parameters[i].EqualsIgnoreCase("tl") && !parameters[i].EqualsIgnoreCase("tr") && !parameters[i].EqualsIgnoreCase("bl") && !parameters[i].EqualsIgnoreCase("br") && !parameters[i].EqualsIgnoreCase("topleft") && !parameters[i].EqualsIgnoreCase("topright") && !parameters[i].EqualsIgnoreCase("bottomleft") && !parameters[i].EqualsIgnoreCase("bottomright"))
            {
                return false;
            }
        }
        return true;
    }

    // Twitch Plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <button> [Presses the specified button] | !{0} press <button> <button> [Example of button chaining] | !{0} reset [Resets all inputs] | Valid buttons are tl, tr, bl, br OR 1-4 being the buttons from in reading order";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*reset\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Debug.LogFormat("[Chinese Counting #{0}] Reset of inputs triggered! (TP)", moduleId);
            stage = 0;
            pressedKeys.Clear();
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length > 1)
            {
                if (cmdIsValid(command))
                {
                    yield return null;
                    for (int i = 1; i < parameters.Length; i++)
                    {
                        if (parameters[i].EqualsIgnoreCase("1"))
                            keys[0].OnInteract();
                        else if (parameters[i].EqualsIgnoreCase("2"))
                            keys[1].OnInteract();
                        else if (parameters[i].EqualsIgnoreCase("3"))
                            keys[2].OnInteract();
                        else if (parameters[i].EqualsIgnoreCase("4"))
                            keys[3].OnInteract();
                        else if (parameters[i].EqualsIgnoreCase("tl") || parameters[i].EqualsIgnoreCase("topleft"))
                            keys[0].OnInteract();
                        else if (parameters[i].EqualsIgnoreCase("tr") || parameters[i].EqualsIgnoreCase("topright"))
                            keys[1].OnInteract();
                        else if (parameters[i].EqualsIgnoreCase("bl") || parameters[i].EqualsIgnoreCase("bottomleft"))
                            keys[2].OnInteract();
                        else if (parameters[i].EqualsIgnoreCase("br") || parameters[i].EqualsIgnoreCase("bottomright"))
                            keys[3].OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = stage; i < 4; i++)
        {
            keys.First(x => x.GetComponentInChildren<TextMesh>().text == correctOrder[pickedNumbersIndexOrdered[i]]).OnInteract();
            yield return new WaitForSeconds(.2f);
        }
    }
}
