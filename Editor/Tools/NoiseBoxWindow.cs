using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;

public class NoiseBoxWindow : EditorWindow {

    NoiseBox noise;
    Texture2D texture;
    int width = 256;
    int height = 256;
    private static Vector2 scrollPosition;

    [OnOpenAssetAttribute(1)]
    public static bool step1(int instanceID, int line)
    {
        UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj is NoiseBox)
        {
            NoiseBoxWindow window = EditorWindow.GetWindow<NoiseBoxWindow>();
            if (window.texture == null)
            {
                window.texture = new Texture2D(window.width, window.height);
            }
            window.noise = (NoiseBox)obj;
            return true;
        }
        return false; // we did not handle the open
    }

    [MenuItem("Assets/Create/Noise")]
    public static void CreateMyAsset()
    {
        NoiseBox asset = ScriptableObject.CreateInstance<NoiseBox>();

        // AssetDatabase.CreateAsset(asset, "Assets/Fighter.asset");
        //AssetDatabase.SaveAssets();
        var path = "";
        var obj = Selection.activeObject;
        if (obj == null) path = "Assets";
        else path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
        if (path.Length > 0 && Directory.Exists(path))
        {
            ProjectWindowUtil.CreateAsset(asset, path + "Noise.asset");
        }
        else
        {
            ProjectWindowUtil.CreateAsset(asset, "Assets/Noise.asset");
        }
        


        //EditorUtility.FocusProjectWindow();

        //Selection.activeObject = asset;
    }

    [MenuItem("Window/Noise Creator %#n")]
    static void Init()
    {
        NoiseBoxWindow window = EditorWindow.GetWindow<NoiseBoxWindow>();
        if (window.texture == null)
        {
            window.texture = new Texture2D(window.width, window.height);
        }
    }

    private void OnGUI()
    {
        noise = (NoiseBox)EditorGUILayout.ObjectField(noise, typeof(NoiseBox), true);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(277), GUILayout.MinWidth(277));
        EditorGUI.DrawPreviewTexture(new Rect(10, 20, 256, 256), texture);
        GUILayout.Space(264);
        TextureSettings();
        if (GUILayout.Button("Rebuild", GUILayout.MaxWidth(150)))
        {
            Rebuild();
        }
        if (GUILayout.Button("Save as PNG", GUILayout.MaxWidth(150)))
        {
            string path = EditorUtility.SaveFilePanel("Save...", "Assets", "texture", "png");
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.Refresh();
            //string normalPath = path.Replace(".png", "_N.png");
            //System.IO.File.WriteAllBytes(normalPath, GetNormal().EncodeToPNG());
        }
        EditorGUILayout.EndVertical();
        if (NoiseBoxEditor(noise))
        {
            Rebuild();
        }
        EditorGUILayout.EndHorizontal();
    }

    void TextureSettings()
    {
        GUILayout.BeginVertical();
        EditorGUI.BeginChangeCheck();
        width = EditorGUILayout.IntField("Width:", width);
        height = EditorGUILayout.IntField("Height", height);
        if (EditorGUI.EndChangeCheck())
        {
            texture = new Texture2D(width, height);
            Rebuild();
        }
        GUILayout.EndVertical();
    }

    void Rebuild()
    {
        noise.Compute();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float xCoord = (i / (float)width);
                float yCoord = (j / (float)height);
                Vector2 r = /*Quaternion.AngleAxis(rotation, Vector3.forward) * */new Vector2(xCoord, yCoord);

                if (noise.colored)
                {
                    Color c = noise.ResolveAsColor(r.x, r.y);
                    texture.SetPixel(i, j, c);
                }
                else
                {
                    float n = noise.Resolve(r.x, r.y);
                    texture.SetPixel(i, j, n * Color.white);
                }
            }
        }
        texture.Apply();
    }

    public static bool NoiseBoxEditor(NoiseBox nb)
    {
        if(nb == null)
        {
            return false;
        }

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add"))
        {
            nb.noises.Add(new NoiseMaterial());
        }
        nb.colored = EditorGUILayout.Toggle("Colored", nb.colored);
        EditorGUILayout.EndHorizontal();
        nb.useEquation = EditorGUILayout.Toggle("Use Equation", nb.useEquation);
        if(nb.useEquation)
        {
            nb.equation = EditorGUILayout.TextField("Equation:", nb.equation);
        }
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginVertical("Box");
        for(int i = 0; i < nb.noises.Count; i+=2){
            EditorGUILayout.BeginHorizontal();
            if(NoiseEditor(nb.noises[i], nb.colored, i))
            {
                nb.noises.RemoveAt(i);
                i--;
            }
            if(i+1 < nb.noises.Count)
            {
                if(NoiseEditor(nb.noises[i + 1], nb.colored, i + 1))
                {
                    nb.noises.RemoveAt(i + 1);
                    i--;
                }
            }
            EditorGUILayout.EndHorizontal(); 
        }
        EditorUtility.SetDirty(nb);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        return EditorGUI.EndChangeCheck();
    }

    static bool NoiseEditor(NoiseMaterial n, bool colored = true, int index = 0)
    {
        EditorGUILayout.BeginVertical("Box", GUILayout.MaxWidth(550));

        EditorGUILayout.BeginHorizontal();
        n.name = EditorGUILayout.TextField("Name:", n.name);
        if(GUILayout.Button("Remove"))
        {
            return true;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if(colored)
        {
            EditorGUILayout.BeginVertical("Box");
            n.upColor = EditorGUILayout.ColorField(n.upColor);
            n.downColor = EditorGUILayout.ColorField(n.downColor);
            EditorGUILayout.EndVertical();
        }
        n.noise.m_frequency = EditorGUILayout.Slider("Frequency:", n.noise.m_frequency, 0, 100);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        n.strength = EditorGUILayout.Slider("Strength:", n.strength, 0, 100);
        n.pureStrength = EditorGUILayout.Slider("Pure Strength:", n.pureStrength, 0, 100);
        n.exageration = EditorGUILayout.Slider("Exageration:", n.exageration, 0, 100);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        n.invert = EditorGUILayout.Toggle("Invert:", n.invert);
        n.rotation = EditorGUILayout.Slider("Rotation:", n.rotation, 0, 360);
        n.scale = EditorGUILayout.Vector2Field("Scale:", n.scale);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        n.noise.m_noiseType = (FastNoise.NoiseType)EditorGUILayout.EnumPopup("Type:", n.noise.m_noiseType);
        n.noise.m_interp = (FastNoise.Interp)EditorGUILayout.EnumPopup("Interpolation:", n.noise.m_interp);
        if(n.noise.m_noiseType == FastNoise.NoiseType.Linear)
        {
            n.noise.m_linearType = (FastNoise.LinearType)EditorGUILayout.EnumPopup("Linear Type:", n.noise.m_linearType);
        }
        if(n.noise.IsFractal())
        {
            EditorGUILayout.BeginVertical("Box");
            n.noise.m_fractalType = (FastNoise.FractalType)EditorGUILayout.EnumPopup("Type:", n.noise.m_fractalType);
            n.noise.SetFractalGain(EditorGUILayout.FloatField("Gain", n.noise.m_gain));
            n.noise.SetFractalLacunarity(EditorGUILayout.FloatField("Lacunarity", n.noise.m_lacunarity));
            n.noise.SetFractalOctaves(EditorGUILayout.IntField("Octave", n.noise.m_octaves));
            EditorGUILayout.EndVertical();
        }
        if(n.noise.m_noiseType == FastNoise.NoiseType.Cellular)
        {
            CellularParameter(n.noise);
        }
        EditorGUILayout.EndVertical();
        n.noise.m_seed = EditorGUILayout.IntField("Seed:", n.noise.m_seed);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        return false;
    }

    static void CellularParameter(FastNoise n)
    {
        EditorGUILayout.BeginVertical("Box");

        n.SetCellularDistanceFunction((FastNoise.CellularDistanceFunction)EditorGUILayout.EnumPopup("Cellular Distance Function:", n.m_cellularDistanceFunction));
        EditorGUILayout.LabelField("Distance Index");

        EditorGUILayout.BeginHorizontal();
        n.m_cellularDistanceIndex0 = EditorGUILayout.IntField(n.m_cellularDistanceIndex0);
        n.m_cellularDistanceIndex1 = EditorGUILayout.IntField(n.m_cellularDistanceIndex1);
        EditorGUILayout.EndHorizontal();

        n.SetCellularJitter(EditorGUILayout.FloatField("Jitter", n.m_cellularJitter));
        n.SetCellularReturnType((FastNoise.CellularReturnType)EditorGUILayout.EnumPopup("Return Type", n.m_cellularReturnType));

        EditorGUILayout.EndVertical();
    }
}
