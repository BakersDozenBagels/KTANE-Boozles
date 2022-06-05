using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System;

/* TODO:
 * Recieve input
 * Display correct inputs
 * Finish manual
 */

public class BandBoozLogic : MonoBehaviour
{
    public KMBombModule Module;
    public BoozleScreenSwitcher switcher;
    public BoozLEDManager leds;
    public TextMesh[] texts;
    public AudioClip[] buttonClips;
    public KMSelectable moduleSelectable;
    public AudioClip startClip, failClip, winClip;
    public KMAudio Audio;
    public MeshRenderer[] buttonRenderers;
    public Material[] materials;
    public KMBombInfo Info;

    private bool _isSolved = false;

    private static int counter = 0;
    private int _id;

    private readonly char[][] topWords = { "clavichord".ToCharArray(), "concertina".ToCharArray(), "bassguitar".ToCharArray(), "didgeridoo".ToCharArray(), "flugelhorn".ToCharArray(), "frenchhorn".ToCharArray() };
    private readonly char[][] bottomWords = { "grandpiano".ToCharArray(), "kettledrum".ToCharArray(), "percussion".ToCharArray(), "sousaphone".ToCharArray(), "tambourine".ToCharArray(), "vibraphone".ToCharArray() };
    private const string ALPHABET = "abcdefghijklmnopqrstuvwxyz0123456789";

    //Usage: TABLE[left][top]
    private readonly char[][] TABLE = { "abcdef".ToCharArray(), "ghijkl".ToCharArray(), "mnopqr".ToCharArray(), "stuvwx".ToCharArray(), "yz1234".ToCharArray(), "567890".ToCharArray() };

    private int CorrectButton = -1;
    private int CorrectTime = -1;
    private bool[] buttonColors = new bool[] { false, false, false, false, false, false };
    private int holdStart = -1;

    private int[] corrects = new int[] { 0, 0, 0, 0 };

    // Use this for initialization
    void Start()
    {
        _id = counter++;
        Generate();
        foreach(KMSelectable button in moduleSelectable.Children)
        {
            button.OnInteract += delegate () { button.GetComponent<ButtonAudio>().PlaySound(); button.transform.localPosition += new Vector3(0f, -0.005f, 0f); return false; };
            button.OnInteractEnded += delegate () { button.transform.localPosition += new Vector3(0f, 0.005f, 0f); PressUp(); };
        }
        for(int i = 0; i < moduleSelectable.Children.Length; i++)
        {
            int j = i;
            moduleSelectable.Children[i].OnInteract += delegate () { Press(j); return false; };
        }
        Module.OnActivate += delegate () { Activate(); };
    }

    private void Activate()
    {
        Audio.PlaySoundAtTransform(startClip.name, transform);
    }

    private void Generate()
    {
        retry:
        int a = UnityEngine.Random.Range(0, 2);
        int b = UnityEngine.Random.Range(0, 6);
        char[] key = new char[10];
        (a == 0 ? topWords : bottomWords)[b].CopyTo(key, 0);
        char[] other = new char[10];
        (a == 1 ? topWords : bottomWords)[b].CopyTo(other, 0);
        int A = UnityEngine.Random.Range(0, 36);
        bool keyloop = UnityEngine.Random.Range(0, 2) == 0;
        bool otherloop = UnityEngine.Random.Range(0, 2) == 0;
        for(int i = 0; i < 10; i++)
        {
            key[i] = ALPHABET[(ALPHABET.IndexOf(key[i]) + (keyloop ? A : ALPHABET.Length - A)) % ALPHABET.Length];
            other[i] = ALPHABET[(ALPHABET.IndexOf(other[i]) + (otherloop ? A : ALPHABET.Length - A)) % ALPHABET.Length];
        }
        int B = UnityEngine.Random.Range(0, 10);
        List<char> keyL = key.Skip(B).ToList();
        keyL.AddRange(key.Take(B));
        key = keyL.ToArray();
        string display = "";
        string log = "";
        foreach(char letter in key)
        {
            display += letter.ToBandzleglyphs();
            log += letter;
        }
        if(display.Take(display.Length / 2).Join("").IsLoop() != keyloop || display.Skip(display.Length / 2).Join("").IsLoop() != otherloop)
            goto retry;
        Debug.LogFormat("[Bandboozled Again #{0}] Key word (decrypted) is: {1}", _id, (a == 0 ? topWords : bottomWords)[b].Join("").ToUpperInvariant());
        Debug.LogFormat("[Bandboozled Again #{0}] A: {1} B: {2}", _id, A, B);
        Debug.LogFormat("[Bandboozled Again #{0}] Encrypted display is: {1}", _id, log);
        Debug.LogFormat("[Bandboozled Again #{0}] The top {1} a useless loop, and the bottom {2}.", _id, display.Take(display.Length / 2).Join("").IsLoop() ? "is" : "is not", display.Skip(display.Length / 2).Join("").IsLoop() ? "is" : "is not");
        switcher.SetMessages(new string[] { "AB" + display.Take(display.Length / 2).Join("") + "YZ\nAB" + display.Skip(display.Length / 2).Join("") + "YZ" });
        List<char> labelsC = ALPHABET.ToCharArray().Where(x => !other.Contains(x)).OrderBy(x => UnityEngine.Random.Range(0, 10000)).Take(5).ToList();
        char c = other.PickRandom();
        labelsC.Add(c);
        labelsC = labelsC.OrderBy(x => UnityEngine.Random.Range(0, 10000)).ToList();
        CorrectButton = labelsC.IndexOf(c);
        for(int i = 0; i < 6; i++)
            for(int j = 5; j >= 0; j--)
                if(labelsC.Contains(TABLE[j][i]))
                    CorrectButton = labelsC.IndexOf(TABLE[j][i]);
        string[] labels = labelsC.Select(x => "AB" + x.ToBandzleglyphs() + "YZ").ToArray();
        Debug.LogFormat("[Bandboozled Again #{0}] Button labels are (reading order) (#! is loop): {1}", _id, labelsC.Select(x => (other.Contains(x) ? "{" : "") + x + (labels[labelsC.IndexOf(x)].IsLoop() ? "!" : "") + (other.Contains(x) ? "}" : "")).Join(", "));
        int soundPosition = -1;
        for(int i = 0; i < 6; i++)
        {
            texts[i].text = labels[i];
            if(other.Contains(labelsC[i])) soundPosition = i;
            buttonColors[i] = UnityEngine.Random.Range(0, 2) == 1;
            buttonRenderers[i].material = buttonColors[i] ? materials[0] : materials[1];
        }
        Debug.LogFormat("[Bandboozled Again #{0}] Button colors (reading order): {1}", _id, buttonColors.Select(x => x ? "Brass" : "Wood").Join(", "));
        for(int i = 0; i < 6; i++)
            buttonColors[i] ^= labels[i].IsLoop();
        int[] soundOrder = new int[] { 0, 1, 2, 3, 4, 5 }.OrderBy(x => UnityEngine.Random.Range(0, 10000)).ToArray();
        foreach(ButtonAudio y in Module.GetComponentsInChildren<ButtonAudio>())
            y.clips = buttonClips.OrderBy(x => soundOrder[Array.IndexOf(buttonClips, x)]).ToArray();
        CorrectTime = Array.IndexOf(soundOrder, soundPosition) + 1;
        Debug.LogFormat("[Bandboozled Again #{0}] Buttons' pitches (lowest to highest): {1}", _id, Enumerable.Range(0,6).Select(s => Array.IndexOf(soundOrder, s) + 1).Join(", "));
        holdStart = B;
        if(A % 2 == 0) buttonColors = buttonColors.Select(x => x ^= true).ToArray();
        neededPressesNow = neededPresses = buttonColors.Count(x => x);
        if(neededPresses <= 0) corrects[3] = 1;

        Debug.LogFormat("[Bandboozled Again #{0}] Expected inputs:\n[Bandboozled Again #{0}] Hold button {1} at {2} for {3} seconds.\n[Bandboozled Again #{0}] Tap the following buttons: {4}", _id, CorrectButton + 1, holdStart, CorrectTime, Enumerable.Range(1, 6).Where(i => buttonColors[i - 1]).Join(", "));
    }

    private float timeDown = -1;
    private bool holdStage = true;
    private int neededPresses = -1;
    private int neededPressesNow = -1;

    private void Press(int input)
    {
        if(_isSolved) return;
        Debug.LogFormat("[Bandboozled Again #{0}] Pressed button {1} on a {2}.", _id, input + 1, Mathf.FloorToInt(Info.GetTime() % 10));
        if(holdStage)
        {
            timeDown = Info.GetTime();
            if(input == CorrectButton) corrects[0] = 1;
            else
            {
                corrects[0] = 0;
                Debug.LogFormat("[Bandboozled Again #{0}] That is wrong, I expected button {1}.", _id, CorrectButton + 1);
            }
            if(Mathf.FloorToInt(timeDown % 10) == holdStart) corrects[1] = 1;
            else
            {
                corrects[1] = 0;
                Debug.LogFormat("[Bandboozled Again #{0}] That is wrong, I you to press it on a {1}.", _id, holdStart);
            }
        }
        else
        {
            if(buttonColors[input])
            {
                buttonColors[input] = false;
                if(neededPresses == neededPressesNow) corrects[3] = 1;
                else if(corrects[3] == 0) corrects[3] = 2;
            }
            else
            {
                if(neededPresses == neededPressesNow) corrects[3] = 0;
                else if(corrects[3] == 1) corrects[3] = 2;
                Debug.LogFormat("[Bandboozled Again #{0}] That is wrong, I did not expect you to press that button.", _id);
            }
            neededPressesNow--;

            Debug.LogFormat("[Bandboozled Again #{0}] Remaining correct presses are: {1}", _id, Enumerable.Range(1, 6).Where(i => buttonColors[i - 1]).Join(", "));
        }
    }

    private void PressUp()
    {
        if(_isSolved) return;
        if(holdStage)
        {
            if(_forceTap)
            {
                Debug.LogFormat("[Bandboozled Again #{0}] Released after 0.1 seconds.", _id, Mathf.Abs(timeDown - Info.GetTime()));
                return;
            }
            else
            {
                Debug.LogFormat("[Bandboozled Again #{0}] Released after {1} seconds.", _id, Mathf.Abs(timeDown - Info.GetTime()));
                if(Mathf.Abs(timeDown - Info.GetTime()) < 0.5f) return;
                holdStage = false;
                if(Mathf.Abs(timeDown - Info.GetTime()) > (CorrectTime - 0.5f) && Mathf.Abs(timeDown - Info.GetTime()) < (CorrectTime + 0.5f)) corrects[2] = 1;
                else
                {
                    Debug.LogFormat("[Bandboozled Again #{0}] That is wrong, I expected {1} seconds.", _id, CorrectTime);
                    corrects[2] = 0;
                }
                Debug.LogFormat("[Bandboozled Again #{0}] Moving on to stage 2. Tap the following buttons: {1}", _id, Enumerable.Range(1, 6).Where(i => buttonColors[i - 1]).Join(", "));
            }
        }
        if(neededPressesNow == 0) CheckInput();
    }

    private void CheckInput()
    {
        leds.ShowState(corrects);
        Debug.LogFormat("[Bandboozled Again #{0}] Submission attempt leds are: {1}", _id, corrects.Select(x => (x == 0) ? "Red" : ((x == 1) ? "Green" : "Yellow")).Join(", "));
        if(corrects.All(x => x == 1)) Solve();
        else StartCoroutine(Strike());
    }

    private void Solve()
    {
        Debug.LogFormat("[Bandboozled Again #{0}] Module solved! Doot doot!", _id);
        Module.HandlePass();
        Audio.PlaySoundAtTransform(winClip.name, transform);
        _isSolved = true;
        StartCoroutine(SolveFanfare());
    }

    private IEnumerator Strike()
    {
        yield return new WaitForSeconds(2f);
        holdStage = true;
        Debug.LogFormat("[Bandboozled Again #{0}] Module strike! Regenerating.", _id);
        Module.HandleStrike();
        Audio.PlaySoundAtTransform(failClip.name, transform);
        Generate();
    }

    public Font solvedFont;
    public Material solvedFontMat;

    private const string WinScr = "Congration,\nYou Did It!";
    private const string WinMsg = "OWO!!!";
    public Material WinMat, WinMat2;

    public TextMesh screen;

    private IEnumerator SolveFanfare()
    {
        yield return new WaitForSeconds(0.5f);
        for(int i = 0; i < texts.Length; i++)
        {
            texts[i].font = solvedFont;
            texts[i].GetComponent<MeshRenderer>().material = solvedFontMat;
            texts[i].text = WinMsg.ToCharArray()[i].ToString();
            buttonRenderers[i].material = WinMat;
            yield return new WaitForSeconds(0.5f);
        }
        screen.font = solvedFont;
        screen.GetComponent<MeshRenderer>().material = solvedFontMat;
        switcher.SetMessages(new string[] { WinScr });
        yield return new WaitForSeconds(5f);
        for(int i = 0; i < texts.Length; i++)
        {
            texts[i].text = "";
            buttonRenderers[i].material = WinMat2;
        }
        switcher.SetMessages(new string[] { "" });
        StopAllCoroutines();
        leds.StopAllCoroutines();
    }

    private bool _forceTap = false;

    //twitch plays
#pragma warning disable 414
    bool ZenModeActive;
    private readonly string TwitchHelpMessage = @"!{0} hold <btn> at <#> for <#₂> [Holds the specified button when the last digit of the bomb's timer is '#' for '#₂' seconds] | !{0} press <btn> (btn2)... [Presses the specified button(s)] | Valid buttons are tl, tm, tr, bl, bm, or br";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if(Regex.IsMatch(parameters[0], @"^\s*hold\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if(parameters.Length == 1 || parameters.Length == 2 || parameters.Length == 3 || parameters.Length == 4 || parameters.Length == 5 || parameters.Length > 6)
            {
                yield return "sendtochaterror Incorrect hold command format! Expected '!{1} hold <btn> at <#> for <#₂>'!";
            }
            else if(parameters.Length == 6)
            {
                if(!parameters[2].EqualsIgnoreCase("at"))
                {
                    yield return "sendtochaterror Incorrect hold command format! Expected '!{1} hold <btn> at <#> for <#₂>' but 'at' was not present!";
                    yield break;
                }
                if(!parameters[4].EqualsIgnoreCase("for"))
                {
                    yield return "sendtochaterror Incorrect hold command format! Expected '!{1} hold <btn> at <#> for <#₂>' but 'for' was not present!";
                    yield break;
                }
                string[] positions = { "tl", "tm", "tr", "bl", "bm", "br" };
                if(!positions.Contains(parameters[1].ToLower()))
                {
                    yield return "sendtochaterror Incorrect hold command format! Expected '!{1} hold <btn> at <#> for <#₂>' but 'btn' is not a valid button!";
                    yield break;
                }
                int temp = 0;
                if(!int.TryParse(parameters[3], out temp))
                {
                    yield return "sendtochaterror Incorrect hold command format! Expected '!{1} hold <btn> at <#> for <#₂>' but '#' is not a valid digit between 0-9!";
                    yield break;
                }
                if(temp < 0 || temp > 9)
                {
                    yield return "sendtochaterror Incorrect hold command format! Expected '!{1} hold <btn> at <#> for <#₂>' but '#' is not a valid digit between 0-9!";
                    yield break;
                }
                int temp2 = 0;
                if(!int.TryParse(parameters[5], out temp2))
                {
                    yield return "sendtochaterror Incorrect hold command format! Expected '!{1} hold <btn> at <#> for <#₂>' but '#₂' is not a valid digit between 1-6!";
                    yield break;
                }
                if(temp2 < 1 || temp2 > 6)
                {
                    yield return "sendtochaterror Incorrect hold command format! Expected '!{1} hold <btn> at <#> for <#₂>' but '#₂' is not a valid digit between 1-6!";
                    yield break;
                }
                while((int)Info.GetTime() % 10 == temp) { yield return "trycancel Halted waiting to hold the button due to a request to cancel!"; }
                while((int)Info.GetTime() % 10 != temp) { yield return "trycancel Halted waiting to hold the button due to a request to cancel!"; }
                moduleSelectable.Children[Array.IndexOf(positions, parameters[1].ToLower())].OnInteract();
                int timer = (int)Info.GetTime();
                if(ZenModeActive)
                    timer += temp2;
                else
                    timer -= temp2;
                while((int)Info.GetTime() != timer) { yield return null; }
                moduleSelectable.Children[Array.IndexOf(positions, parameters[1].ToLower())].OnInteractEnded();
                if(neededPressesNow == 0 && !corrects.All(x => x == 1))
                    yield return "strike";
            }
            yield break;
        }
        if(Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if(parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the button(s) to press!";
            }
            else
            {
                string[] positions = { "tl", "tm", "tr", "bl", "bm", "br" };
                for(int i = 1; i < parameters.Length; i++)
                {
                    if(!positions.Contains(parameters[i].ToLower()))
                    {
                        yield return "sendtochaterror!f The specified button to press '" + parameters[i] + "' is invalid!";
                        yield break;
                    }
                }
                _forceTap = true;
                for(int i = 1; i < parameters.Length; i++)
                {
                    moduleSelectable.Children[Array.IndexOf(positions, parameters[i].ToLower())].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    moduleSelectable.Children[Array.IndexOf(positions, parameters[i].ToLower())].OnInteractEnded();
                    yield return new WaitForSeconds(0.15f);
                }
                _forceTap = false;
                if(neededPressesNow == 0 && !corrects.All(x => x == 1))
                    yield return "strike";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if(!holdStage)
        {
            if(corrects[0] != 1 || corrects[1] != 1 || corrects[2] != 1)
            {
                Module.HandlePass();
                if(neededPressesNow == 0)
                    StopAllCoroutines();
                yield break;
            }
            if(neededPressesNow < neededPresses && corrects[3] != 1)
            {
                Module.HandlePass();
                if(neededPressesNow == 0)
                    StopAllCoroutines();
                yield break;
            }

            for(int i = 0; i < buttonColors.Length; i++)
            {
                if(buttonColors[i])
                {
                    moduleSelectable.Children[i].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    moduleSelectable.Children[i].OnInteractEnded();
                    yield return new WaitForSeconds(0.15f);
                }
            }
        }
        else
        {
            while((int)Info.GetTime() % 10 != holdStart)
                yield return true;
            moduleSelectable.Children[CorrectButton].OnInteract();
            float start = Info.GetTime();
            if(ZenModeActive)
            {
                while(!(Mathf.Abs(start - Info.GetTime()) > CorrectTime - 0.5f && Mathf.Abs(start - Info.GetTime()) < CorrectTime + 0.5f) || (Info.GetTime() - start < 0.5f))
                    yield return null;
            }
            else
            {
                while(!(Mathf.Abs(start - Info.GetTime()) > CorrectTime - 0.5f && Mathf.Abs(start - Info.GetTime()) < CorrectTime + 0.5f) || (start - Info.GetTime() < 0.5f))
                    yield return null;
            }
            moduleSelectable.Children[CorrectButton].OnInteractEnded();
            for(int i = 0; i < buttonColors.Length; i++)
            {
                if(buttonColors[i])
                {
                    moduleSelectable.Children[i].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    moduleSelectable.Children[i].OnInteractEnded();
                    yield return new WaitForSeconds(0.15f);
                }
            }
        }
    }
}

public static class Extensions
{
    //Usage: TABLE[left][top]
    private static readonly char[][] TABLE = { "abcdef".ToCharArray(), "ghijkl".ToCharArray(), "mnopqr".ToCharArray(), "stuvwx".ToCharArray(), "yz1234".ToCharArray(), "567890".ToCharArray() };

    private static readonly string TOP = "abcdef";
    private static readonly string LEFT = "ghijkl";

    public static string ToBandzleglyphs(this char input)
    {
        for(int i = 0; i < 6; i++)
            for(int j = 0; j < 6; j++)
            {
                if(input == TABLE[i][j])
                {
                    return (UnityEngine.Random.Range(0, 1) == 0) ? LEFT[i].ToString() + TOP[j].ToString() : TOP[j].ToString() + LEFT[i].ToString();
                }
            }
        return "";
    }

    public static bool IsLoop(this string input)
    {
        int pos = 0;
        foreach(char x in input)
        {
            if(x == 'b' || x == 'h')
            {
                if(pos == 0) pos = 1;
                else if(pos == 1) pos = 0;
            }
            if(x == 'c' || x == 'f' || x == 'k')
            {
                if(pos == 1) pos = 2;
                else if(pos == 2) pos = 1;
            }
            if(x == 'd')
            {
                if(pos == 0) pos = 2;
                else if(pos == 2) pos = 0;
            }
        }
        return pos == 2;
    }
}