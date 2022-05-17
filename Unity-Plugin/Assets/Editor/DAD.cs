using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System;
using System.Text;

public class DAD : EditorWindow
{
    [MenuItem("DAD/Generate Samples")]
    static void Open()
    {
        GetWindow<DAD>();
    }

    private void OnGUI()
    {
        string s = "";
        s = EditorGUILayout.TextField("Text", s);
        image = (UnityEngine.UI.Image)EditorGUILayout.ObjectField("Image", image, typeof(UnityEngine.UI.Image), true);
        if (GUILayout.Button("Start"))
        {
            ExecutePython();
        }
        if (GUILayout.Button("Read"))
        {
            ReadPython();
        }
        if (GUILayout.Button("Dispose"))
        {
            DisposePython();
        }
    }

    Process process = null;
    UnityEngine.UI.Image image = null;
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
        psi.RedirectStandardError = true;

        process = Process.Start(psi);
        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += (s, e) => { UnityEngine.Debug.Log(e.Data); };
        process.Exited += (s, e) => { DisposePython(); };
    }

    async void ReadPython()
    {
        if (process == null)
        {
            UnityEngine.Debug.Log("process is not on running!");
            return;
        }

        string result = await process.StandardOutput.ReadLineAsync();
        switch (result)
        {
            case "Image":
                result = await process.StandardOutput.ReadLineAsync();
                ImportBase64Image(result, image);
                break;
            case "Obj":
                string name = await process.StandardOutput.ReadLineAsync();
                result = await process.StandardOutput.ReadLineAsync();
                int size = int.Parse(result);
                char[] buf = new char[size];
                await process.StandardOutput.ReadAsync(buf, 0, size);

                result = await process.StandardOutput.ReadLineAsync();
                UnityEngine.Debug.Log(result);
                size = int.Parse(result);
                char[] buf1 = new char[size];
                await process.StandardOutput.ReadAsync(buf1, 0, size);

                ImportOBJ(new string(buf), name, new string(buf1));
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
        image.sprite = null;
        process.Dispose();
        process = null;
    }

    void ImportBase64Image(string s, UnityEngine.UI.Image o)
    {
        byte[] imageBytes = Convert.FromBase64String(s);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes);
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        o.sprite = sprite;
    }

    void ImportOBJ(string s, string name, string mat)
    {
        GameObject o = OBJLoader.LoadOBJFile(s, name, mat);
    }
}
