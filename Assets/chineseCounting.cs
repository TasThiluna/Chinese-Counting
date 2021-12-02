using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class chineseCounting : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    public KMSelectable[] keys;
    private TextMesh[] keyTexts;
    public Renderer[] leds;
    public Material[] ledColors;
    public TextMesh[] colorblindTexts;

    private int[] ledIndices = new int[2];
    private int[] keyLabels = new int[4];
    private int[] solution = new int[4];
    private int stage;

    private static readonly string[] colorNames = new[] { "white", "red", "green", "orange" };
    private static readonly string[] positionNames = new string[] { "top left", "top right", "bottom left", "bottom right" };
    private bool wrong;
    private bool cantPress;
    private List<int> pressedKeys = new List<int>();

    private static int moduleIdCounter = 1;
    private int moduleId;
    private bool moduleSolved;

    private void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable key in keys)
            key.OnInteract += delegate () { PressKey(key); return false; };
        foreach (GameObject t in colorblindTexts.Select(x => x.gameObject))
            t.SetActive(GetComponent<KMColorblindMode>().ColorblindModeActive);
        keyTexts = keys.Select(key => key.GetComponentInChildren<TextMesh>()).ToArray();
    }

    private void Start()
    {
        cantPress = false;
        for (int i = 0; i < 2; i++)
        {
            ledIndices[i] = rnd.Range(0, 4);
            leds[i].material = ledColors[ledIndices[i]];
            colorblindTexts[i].text = "WRGO"[ledIndices[i]].ToString();
        }
        Debug.LogFormat("[Chinese Counting #{0}] The left LED is {1}, and the right LED is {2}.", moduleId, colorNames[ledIndices[0]], colorNames[ledIndices[1]]);
        for (int i = 0; i < 4; i++)
        {
            keyLabels[i] = rnd.Range(0, 101);
            keyTexts[i].text = ChineseNumber(keyLabels[i]);
        }
        Debug.LogFormat("[Chinese Counting #{0}] The labels on the keys are {1}.", moduleId, keyTexts.Select(t => t.text).Join(", "));
        var table = "ACHD,HDAC,CHDA,HACD".Split(',');
        switch (table[ledIndices[0]][ledIndices[1]])
        {
            case 'A':
                solution = keyLabels.Select((x, i) => new { value = x, index = i }).OrderBy(x => x.value).Select(x => x.index).ToArray();
                Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in asending order, by value.", moduleId);
                break;
            case 'D':
                solution = keyLabels.Select((x, i) => new { value = x, index = i }).OrderByDescending(x => x.value).Select(x => x.index).ToArray();
                Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in descending order, by value.", moduleId);
                break;
            case 'C':
                solution = keyLabels.Select((x, i) => new { value = x, index = i }).OrderBy(x => ChineseNumber(x.value).Length).ThenByDescending(x => x.value).Select(x => x.index).ToArray();
                Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in ascending order, by number of characters.", moduleId);
                break;
            case 'H':
                solution = keyLabels.Select((x, i) => new { value = x, index = i }).OrderByDescending(x => ChineseNumber(x.value).Length).ThenBy(x => x.value).Select(x => x.index).ToArray();
                Debug.LogFormat("[Chinese Counting #{0}] The numbers should be pressed in descending order, by number of characters.", moduleId);
                break;
            default:
                throw new Exception("An unexpected value was found in the table.");
        }
        Debug.LogFormat("[Chinese Counting #{0}] Solution: {1}", moduleId, solution.Select(x => positionNames[x]).Join(", "));
    }

    void PressKey(KMSelectable key)
    {
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, key.transform);
        key.AddInteractionPunch(.5f);
        var ix = Array.IndexOf(keys, key);
        if (moduleSolved || cantPress || pressedKeys.Contains(ix))
            return;
        pressedKeys.Add(ix);
        Debug.LogFormat("[Chinese Counting #{0}] You pressed {1}.", moduleId, keyTexts[ix]);
        if (ix != solution[stage])
        {
            module.HandleStrike();
            Debug.LogFormat("[Chinese Counting #{0}] Strike! Resetting...", moduleId);
            pressedKeys.Clear();
            StartCoroutine(Strike());
        }
        else
        {
            stage++;
            if (stage == 4)
            {
                stage = 0;
                module.HandlePass();
                moduleSolved = true;
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                for (int i = 0; i < 2; i++)
                {
                    leds[i].material = ledColors[4];
                    colorblindTexts[i].text = "";
                }
                Debug.LogFormat("[Chinese Counting #{0}] Module solved!", moduleId);
            }
        }
    }

    IEnumerator Strike()
    {
        for (int i = 0; i < 2; i++)
        {
            leds[i].material = ledColors[4];
            colorblindTexts[i].text = "";
        }
        cantPress = true;
        yield return new WaitForSeconds(0.5f);
        Start();
    }

    private static string ChineseNumber(int i)
    {
        var digits = "一二三四五六七八九";
        if (i == 100)
            return "一百";
        else if (i == 200)
            return "二百";
        else if (i > 100 && i < 200)
            return "一百" + ChineseNumber(i - 100);
        else if (i == 0)
            return "〇";
        else if (i.ToString().Length == 1)
            return digits[i - 1].ToString();
        else if (i == 10)
            return "十";
        else if (i < 20)
            return "十" + digits[(i % 10) - 1];
        else if (i % 10 == 0)
            return digits[(i / 10) - 1] + "十";
        else
        {
            var x = digits[int.Parse(i.ToString()[0].ToString()) - 1];
            var y = digits[int.Parse(i.ToString()[1].ToString()) - 1];
            return x + "十" + y;
        }
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
        while (!moduleSolved)
        {
            yield return new WaitForSeconds(.1f);
            keys[solution[stage]].OnInteract();
        }
    }
}
