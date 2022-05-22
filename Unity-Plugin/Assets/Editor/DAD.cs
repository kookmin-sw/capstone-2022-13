using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System;
using System.Text;
using System.IO;

public class DAD : EditorWindow
{
    public DAD()
    {
        theme = "";
        regions = new List<string>();
        characters = new List<string>();
        synopsis = "";
        items = new List<string>();
        quests = new List<string>();
        meshs = new List<Mesh>();
        input = "";
        category = "";
    }

    string theme;
    List<string> regions;
    List<string> characters;
    string synopsis;
    List<string> items;
    List<string> quests;
    List<Mesh> meshs;
    string category;

    string input;

    [MenuItem("DAD/Generate Samples")]
    static void Open()
    {
        GetWindow<DAD>();
    }

    void DrawStringList(string name, List<string> list)
    {
        GUILayout.Label(name);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Length == 0)
            {
                list.RemoveAt(i);
                i--;
                continue;
            }

            list[i] = GUILayout.TextField(list[i]);
        }

        if (GUILayout.Button("Add " + name))
        {
            list.Add(ProcessPython(name.ToLower()));
        }
    }

    Vector2 scroll;

    private void OnGUI()
    {
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.Label("Theme");
        theme = GUILayout.TextField(theme);
        DrawStringList("Region", regions);
        DrawStringList("Character", characters);
        GUILayout.Label("Synopsis");
        synopsis = GUILayout.TextArea(synopsis);
        if (GUILayout.Button("Generate Synopsis"))
        {
            synopsis = ProcessPython("synopsis");
        }
        DrawStringList("Item", items);
        DrawStringList("Quest", quests);
        if (GUILayout.Button("Generate Models"))
        {
            category = "plane";
            ImportOBJ(ProcessPython("model"));
        }
        GUILayout.EndScrollView();
    }

    Process process = null;
    ProcessStartInfo psi;

    void ExecutePython()
    {
        if (process != null)
        {
            UnityEngine.Debug.Log("process is on running!");
            return;
        }

        psi = new ProcessStartInfo();
        psi.FileName = @"C:\Users\dygam\AppData\Local\Programs\Python\Python39\python.exe";
        psi.Arguments = @"C:\Users\dygam\Desktop\test.py";
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardInput = true;
        psi.RedirectStandardError = true;

        process = Process.Start(psi);
        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += (s, e) => { UnityEngine.Debug.Log(e.Data); };
        process.Exited += (s, e) => { UnityEngine.Debug.Log("exited"); };
    }

    string ProcessPython(string mode)
    {
        if (process == null)
        {
            UnityEngine.Debug.Log("process is not on running, execute python process");
            ExecutePython();
        }

        ProcessStandardInputWriteLineInBase64('/' + mode);
        ProcessStandardInputWriteLineInBase64("/theme");
        ProcessStandardInputWriteLineInBase64(theme);
        ProcessStandardInputWriteLineInBase64("/end");
        ProcessStandardInputWriteLineInBase64("/regions");
        foreach (var item in regions)
        {
            ProcessStandardInputWriteLineInBase64(item);
        }
        ProcessStandardInputWriteLineInBase64("/end");
        ProcessStandardInputWriteLineInBase64("/characters");
        foreach (var item in characters)
        {
            ProcessStandardInputWriteLineInBase64(item);
        }
        ProcessStandardInputWriteLineInBase64("/end");
        ProcessStandardInputWriteLineInBase64("/items");
        foreach (var item in items)
        {
            ProcessStandardInputWriteLineInBase64(item);
        }
        ProcessStandardInputWriteLineInBase64("/end");
        ProcessStandardInputWriteLineInBase64("/quests");
        foreach (var item in quests)
        {
            ProcessStandardInputWriteLineInBase64(item);
        }
        ProcessStandardInputWriteLineInBase64("/end");
        ProcessStandardInputWriteLineInBase64("/category");
        ProcessStandardInputWriteLineInBase64(category);
        ProcessStandardInputWriteLineInBase64("/end");
        ProcessStandardInputWriteLineInBase64("/end");
        string t = process.StandardOutput.ReadLine();
        byte[] bytetest = Convert.FromBase64String(t);
        t = Encoding.UTF8.GetString(bytetest);
        return t;
    }

    void ProcessStandardInputWriteLineInBase64(string s)
    {
        byte[] basebyte = System.Text.Encoding.UTF8.GetBytes(s);
        string s64 = Convert.ToBase64String(basebyte);
        process.StandardInput.WriteLine(s64);
    }

    async void ReadPython()
    {
        if (process == null)
        {
            UnityEngine.Debug.Log("process is not on running!");
            return;
        }

        string result = await process.StandardOutput.ReadLineAsync();
        UnityEngine.Debug.Log(result);
        switch (result)
        {
            case "Image":
                result = await process.StandardOutput.ReadLineAsync();
                ImportImageFromFile(result);
                break;
            case "Obj":
                result = await process.StandardOutput.ReadLineAsync();
                ImportOBJ(result);
                break;
        }
    }

    void DisposePython()
    {
        if (process == null)
        {
            UnityEngine.Debug.Log("process is not on running!");
            return;
        }
        process.Dispose();
        process = null;
    }

    void InputPython(string t)
    {
        process.StandardInput.WriteLine(t);
    }

    void ImportBase64Image(string s)
    {
        GameObject o = new GameObject();
        o.transform.parent = FindObjectOfType<Canvas>().transform;
        o.transform.localPosition = Vector3.zero;
        UnityEngine.UI.Image image = o.AddComponent<UnityEngine.UI.Image>();
        byte[] imageBytes = Convert.FromBase64String(s);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes);
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        image.sprite = sprite;
        image.SetNativeSize();
    }

    void ImportImageFromFile(string path)
    {
        GameObject o = new GameObject();
        o.transform.parent = FindObjectOfType<Canvas>().transform;
        o.transform.localPosition = Vector3.zero;
        UnityEngine.UI.Image image = o.AddComponent<UnityEngine.UI.Image>();
        byte[] imageBytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes);
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        image.sprite = sprite;
        image.SetNativeSize();
    }

    void ImportOBJ(string s)
    {
        GameObject o = new GameObject();
        o.AddComponent<MeshFilter>().mesh = OBJLoader.Load(s);
        o.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
    }
}
