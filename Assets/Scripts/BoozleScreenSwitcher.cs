using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BoozleScreenSwitcher : MonoBehaviour {
    public TextMesh text;
    public KMBombModule Module;

    [SerializeField][Multiline]
    private string startMessage = "";

    [SerializeField]
    private float changeDelay;
    private bool hasActivated = false;
    private string[] messages = new string[] { "" };

    private bool isEditorMode = false;
    private string[] debugMessages = new string[] { "" };
    private int editorMax = 0, editorCurrent = 0;

	// Use this for initialization
	void Start () {
        text.text = startMessage;
        Module.OnActivate += delegate () { Activate(); };
	}

    private void Activate()
    {
        hasActivated = true;
        StartCoroutine(CycleMessages());
    }

    public void SetMessages(string[] newMessages)
    {
        StopCoroutine(CycleMessages());
        messages = newMessages;
        if (hasActivated) StartCoroutine(CycleMessages());
    }

    public IEnumerator CycleMessages()
    {
        if (isEditorMode) { EditorGenerate(); yield break; }
        while (true)
        {
            foreach (string message in messages)
            {
                text.text = message;
                yield return new WaitForSeconds(changeDelay);
            }
        }
    }

    public void EditorGenerate()
    {
        if (!isEditorMode) return;
        StopCoroutine(CycleMessages());
        editorMax = debugMessages.Length;
        editorCurrent = 0;
        text.text = debugMessages[0];
    }

    public void EditorNext()
    {
        if (!isEditorMode) return;
        StopCoroutine(CycleMessages());
        editorCurrent++;
        editorCurrent %= editorMax;
        text.text = debugMessages[editorCurrent];
    }

    public void SetMessagesDebug(string[] strings)
    {
        debugMessages = strings;
    }

    public string GetCurrent()
    {
        return text.text;
    }
}
