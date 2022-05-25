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
    public class Item
    {
        public string name;
        public string desc;
        public string type;
        public string effect;
        public string value;

        public Item(string name, string type, string desc, string effect, string value)
        {
            this.name = name;
            this.type = type;
            this.desc = desc;
            this.effect = effect;
            this.value = value;
        }
    }
    public class Quest
    {
        public string name;
        public string desc;
        public List<string> obj;

        public Quest(string name, string desc, List<string> obj)
        {
            this.name = name;
            this.desc = desc;
            this.obj = obj;
        }
    }
    public DAD()
    {
        theme = "";
        regions = new List<string>();
        characters = new List<string>();
        synopsis = "";
        items = new List<Item>();
        quests = new List<Quest>();
        meshs = new List<GameObject>();
        category = "";
    }

    string theme;
    List<string> regions;
    List<string> characters;
    string synopsis;
    List<Item> items;
    List<Quest> quests;
    List<GameObject> meshs;
    string category;

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

    public string ParseXML(string s, string t)
    {
        string f = string.Format("<{0}>", t);
        string l = string.Format("</{0}>", t);
        if (!s.Contains(f))
            return "";
        int p1 = s.IndexOf(f) + f.Length;
        int p2 = s.IndexOf(l);
        return s.Substring(p1, p2 - p1);
    }

    void DrawItemList(string name, List<Item> list)
    {
        GUILayout.Label(name);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].name.Length == 0)
            {
                list.RemoveAt(i);
                i--;
                continue;
            }

            list[i].name = GUILayout.TextField(list[i].name);
            list[i].desc = GUILayout.TextField(list[i].desc);
            list[i].type = GUILayout.TextField(list[i].type);
            list[i].effect = GUILayout.TextField(list[i].effect);
            list[i].value = GUILayout.TextField(list[i].value);
        }

        if (GUILayout.Button("Add " + name))
        {
            string s = ProcessPython(name.ToLower());
            list.Add(new Item(ParseXML(s, "name"), ParseXML(s, "type"), ParseXML(s, "desc"), ParseXML(s, "effect"), ParseXML(s, "value")));
        }
    }

    void DrawQuestList(string name, List<Quest> list)
    {
        GUILayout.Label(name);
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].name.Length == 0)
            {
                list.RemoveAt(i);
                i--;
                continue;
            }

            list[i].name = GUILayout.TextField(list[i].name);
            list[i].desc = GUILayout.TextField(list[i].desc);
            for (int j = 0; j < list[i].obj.Count; j++)
            {
                list[i].obj[j] = GUILayout.TextField(list[i].obj[j]);
            }
        }

        if (GUILayout.Button("Add " + name))
        {
            string s = ProcessPython(name.ToLower());
            List<string> objs = new List<string>();
            int i = 1;
            while(true)
            {
                string z = ParseXML(s, "obj " + i);
                if (z.Length == 0)
                    break;
                objs.Add(z);
                i++;
            }
            list.Add(new Quest(ParseXML(s, "name"), ParseXML(s, "desc"), objs));
        }
    }

    Vector2 scroll;
    Vector2 previewScroll;

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
        DrawItemList("Item", items);
        DrawQuestList("Quest", quests);
        GUILayout.Label("Category");
        category = GUILayout.TextField(category);
        if (GUILayout.Button("Generate Models"))
        {
            foreach (var item in meshs)
            {
                Destroy(item);
            }
            meshs.Clear();
            for (int i = 0; i < 10; i ++)
            {
                string s = ProcessPython("model");
                ImportOBJ(s);
            }
        }
        GUILayout.EndScrollView();
        if (meshs.Count > 0)
        {
            previewScroll = GUILayout.BeginScrollView(previewScroll, true, false);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            foreach (var item in meshs)
            {
                if(GUILayout.Button(AssetPreview.GetAssetPreview(item), GUILayout.Height(100), GUILayout.Width(100)))
                {
                    Instantiate(item);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }
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
        string t = "";
        try
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
                ProcessStandardInputWriteLineInBase64(item.name);
            }
            ProcessStandardInputWriteLineInBase64("/end");
            ProcessStandardInputWriteLineInBase64("/quests");
            foreach (var item in quests)
            {
                ProcessStandardInputWriteLineInBase64(item.name);
            }
            ProcessStandardInputWriteLineInBase64("/end");
            ProcessStandardInputWriteLineInBase64("/category");
            ProcessStandardInputWriteLineInBase64(category);
            ProcessStandardInputWriteLineInBase64("/end");
            ProcessStandardInputWriteLineInBase64("/end");
            t = process.StandardOutput.ReadLine();
            byte[] bytetest = Convert.FromBase64String(t);
            t = Encoding.UTF8.GetString(bytetest);
        }
        catch
        {
            return "null";
        }
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
        o.hideFlags = HideFlags.HideInHierarchy;
        meshs.Add(o);
    }
}
