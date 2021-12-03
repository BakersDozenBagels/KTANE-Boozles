using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KModkit;
using System.Text.RegularExpressions;
using System;

/* TODO:
 * Generate A&B properly
 */

public class RamBoozLogic : MonoBehaviour {
    public KMBombModule Module;
    public BoozleScreenSwitcher switcher;
    public BoozLEDManager leds;
    public TextMesh[] texts;
    public KMSelectable moduleSelectable;
    public AudioClip startClip, failClip, winClip;
    public KMAudio Audio;
    public MeshRenderer[] buttonRenderers;
    public Material[] materials;
    public KMBombInfo Info;

    private bool _isSolved = false;

    private static int counter = 0;
    private int _id;

    private readonly char[][] Words = new string[] { "BillyGoat", "Shearling", "NannyGoat", "Livestock", "Capricorn", "Goatskins", "WaliaIbex", "Shorthair", "Garganica", "Icelandic", "Jamnapari", "Messinese", "Oberhasli", "Norwegian", "Pinzgauer", "SokotoRed", "Repartida", "Blackneck", }.Select(x => x.ToLowerInvariant().ToCharArray()).ToArray();
    private const string ALPHABET = "abcdefghijklmnopqrstuvwxyz";

    //Usage: TABLE[left][top]
    private readonly char[][] TABLE = { "-abcdefgh".ToCharArray(), "ijklmnopq".ToCharArray(), "rstuvwxyz".ToCharArray(), "abcdefghi".ToCharArray(), "jklm*nopq".ToCharArray(), "rstuvwxyz".ToCharArray(), "abcdefghi".ToCharArray(), "jklmnopqr".ToCharArray(), "stuvwxyz+".ToCharArray() };

    private int[] buttonColors = new int[] { -1, -1, -1, -1, -1, -1 };

    private int[] corrects = new int[] { 0, 0, 0, 0 };

    private int[] edgeworkTests = new int[] { -1, -1, -1, -1 };

    private int screenVal = -1;

    private List<int> labelsVals = new int[] { -1, -1, -1, -1, -1, -1 }.ToList();

    // Use this for initialization
    void Start () {
        _id = counter++;
        Generate();
        foreach (KMSelectable button in moduleSelectable.Children)
        {
            button.OnInteract += delegate () { button.transform.localPosition += new Vector3(0f, -0.005f, 0f); return false; };
            button.OnInteractEnded += delegate () { button.transform.localPosition += new Vector3(0f, 0.005f, 0f); PressUp(); };
        }
        for (int i = 0; i < moduleSelectable.Children.Length; i++)
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
        int on = Info.GetOnIndicators().Count();
        int off = Info.GetOffIndicators().Count();
        int AA = Info.GetBatteryCount(Battery.AA) / 2;
        int D = Info.GetBatteryCount(Battery.D);
        edgeworkTests[0] = (off + 10) % 10;
        edgeworkTests[1] = (AA + 10) % 10;
        edgeworkTests[2] = (D + 10) % 10;
        edgeworkTests[3] = (on + 10) % 10;

        Debug.LogFormat("<Ramboozled Again #{0}> Edgework tests are: {1}", _id, edgeworkTests.Join(", "));

        List<char> labelsC = ALPHABET.ToCharArray().OrderBy(x => UnityEngine.Random.Range(0, 10000)).Take(6).ToList();
        char[] labelsC2 = new char[] { ' ', ' ', ' ', ' ', ' ', ' ' };
        labelsVals = new int[] { -1, -1, -1, -1, -1, -1 }.ToList();
        for (int i = 0; i < 6; i++)
        {
            while (ALPHABET.IsNull(labelsC2[i]) ? true : labelsVals.Contains((ALPHABET.IndexOf(labelsC[i]) + ALPHABET.IndexOf(labelsC2[i]) + 2) % 26))
            {
                labelsC2[i] = ALPHABET.PickRandom();
            }
            labelsVals[i] = (ALPHABET.IndexOf(labelsC[i]) + ALPHABET.IndexOf(labelsC2[i]) + 2) % 26;
        }
        labelsVals = labelsC.Select(x => (ALPHABET.IndexOf(x) + ALPHABET.IndexOf(labelsC2[labelsC.IndexOf(x)]) + 2) % 26).ToList();
        string[] labels = labelsC.Select(x => x.ToRamzleglyphs(' ') + "\n" + labelsC2[labelsC.IndexOf(x)].ToRamzleglyphs(x)).ToArray();
        Debug.LogFormat("[Ramboozled Again #{0}] Button labels are (reading order): {1}", _id, labelsC.Select(x => x.ToString() + labelsC2[labelsC.IndexOf(x)]).Join(", "));
        Debug.LogFormat("<Ramboozled Again #{0}> Button glyphs are: {1}", _id, labels.Join(", "));
        int b = UnityEngine.Random.Range(0, 18);
        char[] key = Words[b];
        Debug.LogFormat("[Ramboozled Again #{0}] Key word (decrypted) is: {1}", _id, key.Join("").ToUpperInvariant());
        int A, B;
        do
        {
            A = UnityEngine.Random.Range(0, 26);
            B = UnityEngine.Random.Range(0, 9);
        }
        while (!labelsVals.Contains((A * B) % 26));
        for (int i = 0; i < 9; i++)
        {
            key[i] = ALPHABET.ToCharArray()[(ALPHABET.IndexOf(key[i]) + A) % ALPHABET.Length];
        }
        Debug.LogFormat("[Ramboozled Again #{0}] A: {1} B: {2}", _id, A, B);
        screenVal = (A * B) % 26;
        List<char> keyL = key.Skip(B).ToList();
        keyL.AddRange(key.Take(B));
        key = keyL.ToArray();
        string display = "";
        char prev = ' ';
        foreach (char letter in key)
        {
            display += letter.ToRamzleglyphs(prev);
            prev = letter;
        }
        Debug.LogFormat("<Ramboozled Again #{0}> Display glyphs are: {1}", _id, display);
        switcher.SetMessages(new string[] { display.Take(display.Length/3).Join("") + "\n" + display.Skip(display.Length / 3).Take(display.Length / 3).Join("") + "\n" + display.Skip(display.Length * 2 / 3).Join("") });
        for (int i = 0; i < 6; i++)
        {
            texts[i].text = labels[i];
            buttonColors[i] = UnityEngine.Random.Range(0, 4);
            buttonRenderers[i].material = materials[buttonColors[i]];
        }
        Debug.LogFormat("[Ramboozled Again #{0}] Button colors (reading order): {1}", _id, buttonColors.Select(x => x == 0 ? "Black" : (x == 1 ? "Brown" : (x == 2 ? "Tan" : "White"))).Join(", "));
        neededPresses = neededPressesNow = 4;
    }

    private float timeDown = -1;
    private int neededPresses = -1;
    private int neededPressesNow = -1;
    private int pressedButtonId = -1;
    private bool isCorrectButton = false;

    private void Press(int input)
    {
        if (_isSolved) return;
        Debug.LogFormat("[Ramboozled Again #{0}] Pressed button {1} on a {2}.", _id, input + 1, Mathf.FloorToInt(Info.GetTime() % 10));
        timeDown = Info.GetTime();
        pressedButtonId = input;
        isCorrectButton = labelsVals[input] == screenVal;
    }

    private void PressUp()
    {
        if (_isSolved) return;
        Debug.LogFormat("[Ramboozled Again #{0}] Released after {1} seconds.", _id, Mathf.Abs(timeDown - Info.GetTime()));
        if (Mathf.Abs(timeDown - Info.GetTime()) < 0.5f) return;
        if (Mathf.Floor(timeDown) % 10 > (edgeworkTests[buttonColors[pressedButtonId]] - 0.5f) && Mathf.Floor(timeDown) % 10 < (edgeworkTests[buttonColors[pressedButtonId]] + 0.5f) && isCorrectButton) corrects[4 - neededPressesNow] = 1;
        else if (isCorrectButton) corrects[4 - neededPressesNow] = 2;
        else corrects[2] = 0;
        neededPressesNow--;
        if (neededPressesNow == 0) CheckInput();
        else
        {
            int b = UnityEngine.Random.Range(0, 18);
            char[] key = new char[9];
            Words[b].CopyTo(key, 0);
            Debug.LogFormat("[Ramboozled Again #{0}] Key word (decrypted) is: {1}", _id, key.Join("").ToUpperInvariant());
            int A, B;
            do
            {
                A = UnityEngine.Random.Range(0, 36);
                B = UnityEngine.Random.Range(0, 9);
            }
            while (!labelsVals.Contains((A * B) % 26));
            for (int i = 0; i < 9; i++)
            {
                key[i] = ALPHABET.ToCharArray()[(ALPHABET.IndexOf(key[i]) + A) % ALPHABET.Length];
            }
            Debug.LogFormat("[Ramboozled Again #{0}] A: {1} B: {2}", _id, A, B);
            screenVal = (A * B) % 26;
            List<char> keyL = key.Skip(B).ToList();
            keyL.AddRange(key.Take(B));
            key = keyL.ToArray();
            string display = "";
            char prev = ' ';
            foreach (char letter in key)
            {
                display += letter.ToRamzleglyphs(prev);
                prev = letter;
            }
            Debug.LogFormat("<Ramboozled Again #{0}> Display glyphs are: {1}", _id, display);
            switcher.SetMessages(new string[] { display.Take(display.Length / 3).Join("") + "\n" + display.Skip(display.Length / 3).Take(display.Length / 3).Join("") + "\n" + display.Skip(display.Length * 2 / 3).Join("") });
        }
    }

    private void CheckInput()
    {
        leds.ShowState(corrects);
        Debug.LogFormat("[Ramboozled Again #{0}] Submission attempt leds are: {1}", _id, corrects.Select(x => (x == 0) ? "Red" : ((x == 1) ? "Green" : "Yellow")).Join(", "));
        if (corrects.All(x => x == 1)) Solve();
        else StartCoroutine(Strike());
    }

    private void Solve()
    {
        Debug.LogFormat("[Ramboozled Again #{0}] Module solved! *Bleats happily*", _id);
        Module.HandlePass();
        Audio.PlaySoundAtTransform(winClip.name, transform);
        _isSolved = true;
        StartCoroutine(SolveFanfare());
    }

    private IEnumerator Strike()
    {
        yield return new WaitForSeconds(2f);
        Debug.LogFormat("[Ramboozled Again #{0}] Module strike! Regenerating.", _id);
        Module.HandleStrike();
        Audio.PlaySoundAtTransform(failClip.name, transform);
        Generate();
        neededPresses = 4;
    }

    public Font solvedFont;
    public Material solvedFontMat;

    private const string WinScr = "Congration,\nYou Done It!";
    private const string WinMsg = "O!O!W!";
    public Material WinMat, WinMat2;

    public TextMesh screen;

    private IEnumerator SolveFanfare()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < texts.Length; i++)
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
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = "";
            buttonRenderers[i].material = WinMat2;
        }
        switcher.SetMessages(new string[] { "" });
        StopAllCoroutines();
        leds.StopAllCoroutines();
    }

    //twitch plays
    #pragma warning disable 414
    bool ZenModeActive;
    private readonly string TwitchHelpMessage = @"!{0} press <btn> at <#> [Presses the specified button when the last digit of the bomb's timer is '#'] | Valid buttons are tl, tm, tr, bl, bm, or br";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length == 1 || parameters.Length == 2 || parameters.Length == 3 || parameters.Length > 4)
            {
                yield return "sendtochaterror Incorrect press command format! Expected '!{1} press <btn> at <#>'!";
            }
            else if (parameters.Length == 4)
            {
                if (!parameters[2].EqualsIgnoreCase("at"))
                {
                    yield return "sendtochaterror Incorrect press command format! Expected '!{1} press <btn> at <#>' but 'at' was not present!";
                    yield break;
                }
                string[] positions = { "tl", "tm", "tr", "bl", "bm", "br" };
                if (!positions.Contains(parameters[1].ToLower()))
                {
                    yield return "sendtochaterror Incorrect press command format! Expected '!{1} press <btn> at <#>' but 'btn' is not a valid button!";
                    yield break;
                }
                int temp = 0;
                if (!int.TryParse(parameters[3], out temp))
                {
                    yield return "sendtochaterror Incorrect press command format! Expected '!{1} press <btn> at <#>' but '#' is not a valid digit between 0-9!";
                    yield break;
                }
                if (temp < 0 || temp > 9)
                {
                    yield return "sendtochaterror Incorrect press command format! Expected '!{1} press <btn> at <#>' but '#' is not a valid digit between 0-9!";
                    yield break;
                }
                while ((int)Info.GetTime() % 10 == temp) { yield return "trycancel Halted waiting to press the button due to a request to cancel!"; }
                while ((int)Info.GetTime() % 10 != temp) { yield return "trycancel Halted waiting to press the button due to a request to cancel!"; }
                moduleSelectable.Children[Array.IndexOf(positions, parameters[1].ToLower())].OnInteract();
                float timer = Info.GetTime();
                if (ZenModeActive)
                    while (Info.GetTime() <= (timer + 0.5f)) { yield return null; }
                else
                    while (Info.GetTime() >= (timer - 0.5f)) { yield return null; }
                moduleSelectable.Children[Array.IndexOf(positions, parameters[1].ToLower())].OnInteractEnded();
                if (neededPressesNow == 0 && !corrects.All(x => x == 1))
                    yield return "strike";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (neededPressesNow == 0)
        {
            Module.HandlePass();
            StopAllCoroutines();
            yield break;
        }
        int presses = neededPresses - neededPressesNow;
        for (int i = 0; i < presses; i++)
        {
            if (corrects[i] != 1)
            {
                Module.HandlePass();
                yield break;
            }
        }

        for (int j = presses; j < 4; j++)
        {
            for (int i = 0; i < 6; i++)
            {
                if (labelsVals[i] == screenVal)
                {
                    while ((int)Info.GetTime() % 10 != edgeworkTests[buttonColors[i]])
                        yield return true;
                    moduleSelectable.Children[i].OnInteract();
                    float start = Info.GetTime();
                    if (ZenModeActive)
                    {
                        while ((Info.GetTime() - start) < 0.5f)
                            yield return null;
                    }
                    else
                    {
                        while ((start - Info.GetTime()) < 0.5f)
                            yield return null;
                    }
                    moduleSelectable.Children[i].OnInteractEnded();
                    break;
                }
            }
        }
    }
}

public static class RamExtensions
{
    //Usage: TABLE[left][top]
    private static readonly char[][] TABLE = new string[] { "-abcdefgh", "ijklmnopq", "rstuvwxyz", "abcdefghi", "jklm*nopq", "rstuvwxyz", "abcdefghi", "jklmnopqr", "stuvwxyz+" }.Select(x => x.ToCharArray()).ToArray();

    private static readonly char[] TOP = "ABCDEFGHI".ToCharArray();
    private static readonly char[] LEFT = "abcdefghi".ToCharArray();
    private static readonly char[] ALPHABET = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

    public static string ToRamzleglyphs(this char Input, char Previous)
    {
        List<string> results = new List<string>();
        if (Previous != ' ')
        {
            if ((System.Array.IndexOf(ALPHABET, Previous) + 1) % 26 == System.Array.IndexOf(ALPHABET, Input)) return "Ii";
            if ((System.Array.IndexOf(ALPHABET, Previous) - 1) % 26 == System.Array.IndexOf(ALPHABET, Input)) return "Aa";
            if ((System.Array.IndexOf(ALPHABET, Previous) + 13) % 26 == System.Array.IndexOf(ALPHABET, Input)) return "Ee";
        }
        foreach (char[] row in TABLE)
        {
            foreach (char item in row)
            {
                if (item == Input) results.Add(TOP[System.Array.IndexOf(row, item)].ToString() + LEFT[System.Array.IndexOf(TABLE, row)].ToString());
            }
        }
        return results.OrderBy(x => UnityEngine.Random.Range(0, 10000)).FirstOrDefault();
    }

    public static bool IsNull(this string str, char ch)
    {
        return !str.Contains(ch);
    }
}