using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBJLoader : MonoBehaviour
{
    static int[] quadToTriangle = new int[]
    {
        0, 1, 2,
        0, 2, 3
    };

    public static Mesh Load(string path)
    {
        string text = System.IO.File.ReadAllText(path);
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        foreach (string line in text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            string[] splitLine = line.Split(' ');
            switch (splitLine[0])
            {
                case "v":
                    vertices.Add(new Vector3(float.Parse(splitLine[1]), float.Parse(splitLine[2]), float.Parse(splitLine[3])));
                    break;

                case "f":
                    {
                        int[] tmpIndices = new int[splitLine.Length - 1];
                        for (int i = 0; i < splitLine.Length - 1; ++i)
                        {
                            tmpIndices[i] = int.Parse(splitLine[i + 1].Split('/')[0]) - 1;
                        }

                        switch (tmpIndices.Length)
                        {
                            case 3:
                                indices.AddRange(tmpIndices);
                                break;

                            case 4:
                                foreach (int i in quadToTriangle)
                                {
                                    indices.Add(tmpIndices[i]);
                                }
                                break;
                        }
                        break;
                    }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
