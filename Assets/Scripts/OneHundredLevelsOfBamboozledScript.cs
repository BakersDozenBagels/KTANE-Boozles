using KeepCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneHundredLevelsOfBamboozledScript : ModuleScript
{
    public new KMBombModule Module;
    public BoozleScreenSwitcher switcher;
    public BoozLEDManager leds;
    public TextMesh[] texts;
    public KMSelectable moduleSelectable;
    public AudioClip startClip, failClip, winClip;
    public KMAudio Audio;
    public MeshRenderer[] buttonRenderers;
    public Material[] materials;
    public KMBombInfo Info;

    private int _requiredSolves = -1, _level = -1;
    private bool _generated = false;
    private static string[] _previousLevels;
    private static string _previousLevel;

    private static readonly int[] _requiredSolvesByLevel = new[] { -1,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
    };

    private void Start()
    {
        string mission = Game.Mission.DisplayName;
        if(mission.Substring(0, 6) != "Level " && mission != "The First Bomb")
        {
            GenerateStandard();
            return;
        }

        if(!IsEditor && !int.TryParse(mission.Substring(6).SkipLast(1).Join(""), out _level))
        {
            GenerateStandard();
            return;
        }
        if(IsEditor)
            _level = 1;

        LoadFromFile();
        Log("All previous words: " + _previousLevels.Join(" "));

        if(_level >= 2 && _previousLevels[_level-2] == "")
        {
            Game.AddStrikes(gameObject, Game.Mission.GeneratorSetting.NumStrikes, true);
            return;
        }

        _previousLevel = _level == 1 ? "GOODLUCK" : _previousLevels[_level - 2];

        switcher.SetMessages(new string[] { "Welcome to:", "Level " + _level });
        foreach(KMSelectable b in moduleSelectable.Children)
            b.OnInteract += () => { return false; };
        foreach(MeshRenderer m in buttonRenderers)
            m.material = materials[0];
        foreach(TextMesh t in texts)
            t.text = "";

        _requiredSolves = _requiredSolvesByLevel[_level];

        Log(_requiredSolves + " solves are required to unlock the module. Best of luck...");
        if(!_generated && _level != -1 && _requiredSolves <= Info.GetSolvedModuleNames().Count)
            GenerateLevel();
    }

    public override void OnModuleSolved(ModuleContainer module)
    {
        if(!_generated && _level != -1 && _requiredSolves <= Info.GetSolvedModuleNames().Count)
            GenerateLevel();
    }

    private void GenerateLevel()
    {
        _generated = true;
        buttonRenderers[4].material = materials[1];
        texts[4].text = "GO";
        moduleSelectable.Children[4].OnInteract += () => { Initialize(); return false; };
    }

    private void Initialize()
    {
        throw new NotImplementedException();
    }

    private void GenerateStandard()
    {
        throw new NotImplementedException();
    }

    private void WriteToFile()
    {
        if(IsEditor || _previousLevels == null)
            return;

        string path = null;
        try
        {
            path = PathManager.GetPath("100lob.persistentstorage");
            if(path.IsNullOrEmpty())
                throw new System.IO.FileNotFoundException();

        }
        catch(System.IO.FileNotFoundException)
        {
            path = PathManager.CombineMultiple(PathManager.GetDirectory(), "100lob.persistentstorage");
            System.IO.FileStream fs = System.IO.File.Create(path);
            fs.Close();
        }

        System.IO.File.WriteAllText(path, _previousLevels.Join("\n"));
    }

    private void LoadFromFile(bool force = false)
    {
        if(IsEditor)
        {
            _previousLevels = new string[100];
            return;
        }
        if(!force && _previousLevels != null)
            return;

        string path = null;
        try
        {
            path = PathManager.GetPath("100lob.persistentstorage");
            if(path.IsNullOrEmpty())
                throw new System.IO.FileNotFoundException();
        }
        catch(System.IO.FileNotFoundException)
        {
            path = PathManager.CombineMultiple(PathManager.GetDirectory(), "100lob.persistentstorage");
            System.IO.FileStream fs = System.IO.File.Create(path);
            fs.Close();

            _previousLevels = new string[100];
            return;
        }

        _previousLevels = System.IO.File.ReadAllLines(path);

    }
}
