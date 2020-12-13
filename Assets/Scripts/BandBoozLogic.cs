using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/* TODO:
 * Recieve input
 * Display correct inputs
 * Finish manual
 */

public class BandBoozLogic : MonoBehaviour {
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
    void Start () {
        _id = counter++;
        Generate();
        foreach (KMSelectable button in moduleSelectable.Children)
        {
            button.OnInteract += delegate () { button.GetComponent<ButtonAudio>().PlaySound(); button.transform.localPosition += new Vector3(0f, -0.005f, 0f); return false; };
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
        int a = Random.Range(0, 2);
        int b = Random.Range(0, 6);
        char[] key = (a == 0 ? topWords : bottomWords)[b];
        Debug.LogFormat("[Bandboozled Again #{0}] Key word (decrypted) is: {1}", _id, key.Join("").ToUpperInvariant());
        char[] other = (a == 1 ? topWords : bottomWords)[b];
        int A = Random.Range(0, 36);
        bool keyloop = key.ToString().IsLoop();
        bool otherloop = other.ToString().IsLoop();
        for (int i = 0; i < 10; i++)
        {
            key[i] = ALPHABET[(ALPHABET.IndexOf(key[i]) + (keyloop ? A : ALPHABET.Length - A)) % ALPHABET.Length];
            other[i] = ALPHABET[(ALPHABET.IndexOf(other[i]) + (otherloop ? A : ALPHABET.Length - A)) % ALPHABET.Length];
        }
        int B = Random.Range(0, 10);
        Debug.LogFormat("[Bandboozled Again #{0}] A: {1} B: {2}", _id, A, B);
        List<char> keyL = key.Skip(B).ToList();
        keyL.AddRange(key.Take(B));
        key = keyL.ToArray();
        string display = "";
        foreach (char letter in key)
        {
            display += letter.ToBandzleglyphs();
        }
        switcher.SetMessages(new string[] { "AB" + display.Take(display.Length/2).Join("") + "YZ\nAB" + display.Skip(display.Length / 2).Join("") + "YZ" });
        List<char> labelsC = ALPHABET.ToCharArray().Where(x => !other.Contains(x)).OrderBy(x => Random.Range(0, 10000)).Take(5).ToList();
        char c = other.PickRandom();
        labelsC.Add(c);
        labelsC = labelsC.OrderBy(x => Random.Range(0, 10000)).ToList();
        CorrectButton = labelsC.IndexOf(c);
        for (int i = 0; i < 6; i++)
            for (int j = 5; j >= 0; j--)
                if (labelsC.Contains(TABLE[j][i]))
                    CorrectButton = labelsC.IndexOf(TABLE[j][i]);
        string[] labels = labelsC.Select(x => "AB" + x.ToBandzleglyphs() + "YZ").ToArray();
        Debug.LogFormat("[Bandboozled Again #{0}] Button labels are (reading order) (#! is loop): {1}", _id, labelsC.Select(x => (other.Contains(x) ? "{" : "") + x + (labels[labelsC.IndexOf(x)].IsLoop() ? "!" : "") + (other.Contains(x) ? "}" : "")).Join(", "));
        int soundPosition = -1;
        for (int i = 0; i < 6; i++)
        {
            texts[i].text = labels[i];
            if (other.Contains(labelsC[i])) soundPosition = i;
            buttonColors[i] = Random.Range(0, 2) == 1;
            buttonRenderers[i].material = buttonColors[i] ? materials[0] : materials[1];
        }
        Debug.LogFormat("[Bandboozled Again #{0}] Button colors (reading order): {1}", _id, buttonColors.Select(x => x ? "Brass" : "Wood").Join(", "));
        for (int i = 0; i < 6; i++)
            buttonColors[i] ^= labels[i].IsLoop();
        int[] soundOrder = new int[] { 0, 1, 2, 3, 4, 5 }.OrderBy(x => Random.Range(0, 10000)).ToArray();
        foreach (ButtonAudio y in Module.GetComponentsInChildren<ButtonAudio>())
            y.clips = buttonClips.OrderBy(x => soundOrder[System.Array.IndexOf(buttonClips, x)]).ToArray();
        CorrectTime = System.Array.IndexOf(soundOrder, soundPosition) + 1;
        Debug.LogFormat("[Bandboozled Again #{0}] Pitches' buttons (lowest to highest): {1}", _id, soundOrder.Select(x => x + 1).Join(", "));
        holdStart = B;
        if (A % 2 == 0) buttonColors = buttonColors.Select(x => x ^= true).ToArray();
        neededPressesNow = neededPresses = buttonColors.Count(x => x);
        if (neededPresses <= 0) corrects[3] = 1;
    }

    private float timeDown = -1;
    private bool holdStage = true;
    private int neededPresses = -1;
    private int neededPressesNow = -1;

    private void Press(int input)
    {
        if (_isSolved) return;
        Debug.LogFormat("[Bandboozled Again #{0}] Pressed button {1} on a {2}.", _id, input + 1, Mathf.FloorToInt(Info.GetTime() % 10));
        if (holdStage)
        {
            timeDown = Info.GetTime();
            if (input == CorrectButton) corrects[0] = 1;
            else corrects[0] = 0;
            if (Mathf.FloorToInt(timeDown % 10) == holdStart) corrects[1] = 1;
            else corrects[1] = 0;
        }
        else
        {
            if (buttonColors[input])
            {
                buttonColors[input] = false;
                if (neededPresses == neededPressesNow) corrects[3] = 1;
                else if (corrects[3] == 0) corrects[3] = 2;
            }
            else
            {
                if (neededPresses == neededPressesNow) corrects[3] = 0;
                else if (corrects[3] == 1) corrects[3] = 2;
            }
            neededPressesNow--;
        }
    }

    private void PressUp()
    {
        if (_isSolved) return;
        if (holdStage)
        {
            Debug.LogFormat("[Bandboozled Again #{0}] Released after {1} seconds.", _id, Mathf.Abs(timeDown - Info.GetTime()));
            if (Mathf.Abs(timeDown - Info.GetTime()) < 0.5f) return;
            holdStage = false;
            if (Mathf.Abs(timeDown - Info.GetTime()) > (CorrectTime - 0.5f) && Mathf.Abs(timeDown - Info.GetTime()) < (CorrectTime + 0.5f)) corrects[2] = 1;
            else corrects[2] = 0;
        }
        if (neededPressesNow == 0) CheckInput();
    }

    private void CheckInput()
    {
        leds.ShowState(corrects);
        Debug.LogFormat("[Bandboozled Again #{0}] Submission attempt leds are: {1}", _id, corrects.Select(x => (x==0)?"Red":((x==1)?"Green":"Yellow")).Join(", "));
        if (corrects.All(x => x == 1)) Solve();
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
}

public static class Extensions
{
    //Usage: TABLE[left][top]
    private static readonly char[][] TABLE = { "abcdef".ToCharArray(), "ghijkl".ToCharArray(), "mnopqr".ToCharArray(), "stuvwx".ToCharArray(), "yz1234".ToCharArray(), "567890".ToCharArray() };

    private static readonly string TOP = "abcdef";
    private static readonly string LEFT = "ghijkl";

    public static string ToBandzleglyphs(this char input)
    {
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
            {
                if (input == TABLE[i][j])
                {
                    return (Random.Range(0, 1) == 0) ? LEFT[i].ToString() + TOP[j].ToString() : TOP[j].ToString() + LEFT[i].ToString();
                }
            }
        return "";
    }

    public static bool IsLoop(this string input)
    {
        int pos = 0;
        foreach (char x in input)
        {
            if (x == 'b' || x == 'h')
            {
                if (pos == 0) pos = 1;
                else if (pos == 1) pos = 0;
            }
            if (x == 'c' || x == 'f' || x == 'k')
            {
                if (pos == 1) pos = 2;
                else if (pos == 2) pos = 1;
            }
            if (x == 'd')
            {
                if (pos == 0) pos = 2;
                else if (pos == 2) pos = 0;
            }
        }
        return pos == 2;
    }
}