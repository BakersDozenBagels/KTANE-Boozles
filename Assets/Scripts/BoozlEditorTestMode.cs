using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;

public class BoozlEditorTestMode : MonoBehaviour  {

#if UNITY_EDITOR

    public KMSelectable[] buttons;
    public BoozLEDManager manager;
    public BoozleScreenSwitcher switcher;
    public KMAudio Audio;
    public AudioClip[] debugSounds;

    private KMSelectable.OnInteractHandler[] funcs = new KMSelectable.OnInteractHandler[6];
    private KMSelectable.OnInteractHandler[] newFuncs = new KMSelectable.OnInteractHandler[6];
    private System.Action[] ended = new System.Action[6];

    private void Start()
    {
        Audio.PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.CorrectChime, transform);
        Debug.Log("EDITOR MODE, USE BUTTONS TO:\nFLASH RED        FLASH GREEN       SWITCH SOUND\nFLASH YELLOW     PLAY SOUND        EXIT MODE");
        newFuncs[0] = new KMSelectable.OnInteractHandler(delegate () { manager.ShowState(new int[] { 0, 0, 0, 0 }); return false; });
        newFuncs[1] = new KMSelectable.OnInteractHandler(delegate () { manager.ShowState(new int[] { 1, 1, 1, 1 }); return false; });
        newFuncs[2] = new KMSelectable.OnInteractHandler(delegate () { switcher.EditorNext(); return false; });
        newFuncs[3] = new KMSelectable.OnInteractHandler(delegate () { manager.ShowState(new int[] { 2, 2, 2, 2 }); return false; });
        newFuncs[4] = new KMSelectable.OnInteractHandler(delegate () { Audio.PlaySoundAtTransform(switcher.GetCurrent(), transform); return false; });
        newFuncs[5] = new KMSelectable.OnInteractHandler(delegate () { ResetButtons(); return false; });
        StartCoroutine(MakeButtons());
    }

    private IEnumerator MakeButtons()
    {
        yield return null;
        for (int i = 0; i < 6; i++)
        {
            funcs[i] = buttons[i].OnInteract;
            buttons[i].OnInteract = newFuncs[i];
            ended[i] = buttons[i].OnInteractEnded;
            buttons[i].OnInteractEnded = delegate () { };
        }
        switcher.GetType().GetField("isEditorMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(switcher, true);
        switcher.SetMessagesDebug(debugSounds.Select(x => x.name).ToArray());
        switcher.EditorGenerate();
    }

    private void ResetButtons()
    {
        for (int i = 0; i < 6; i++)
        {
            buttons[i].OnInteract = funcs[i];
            buttons[i].OnInteractEnded = ended[i];
        }
        buttons[5].OnInteract();
        switcher.GetType().GetField("isEditorMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(switcher, false);
        switcher.StartCoroutine(switcher.CycleMessages());
    }

#endif

}
