using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;


public class TextureCreatorWindow : EditorWindow
{
    Texture2D texture;
    Color fillColor;
    Color secondary;
    FastNoise noise = new FastNoise();
    NoiseBox noiseBox;

    float perlinScale1 = 1.0f;
    int width = 1024;
    int height = 1024;
    Mesh mesh;
    Material mat;

    float rotation;
    Vector2 offset;
    private Vector2 scrollPosition;

    [MenuItem("Window/Texture Creator %#t")]
    static void Init()
    {
        TextureCreatorWindow window = EditorWindow.GetWindow<TextureCreatorWindow>();
        if(window.texture == null)
        {
            window.texture = new Texture2D(64, 64);
        }
        window.mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Library/unity default resources::Cube");
        window.mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
    }

    void OnGUI()
    {
        GUILayout.Label("Texture Creation", EditorStyles.boldLabel);
        noiseBox = (NoiseBox)EditorGUILayout.ObjectField(noiseBox, typeof(NoiseBox), true);
        EditorGUI.DrawPreviewTexture(new Rect(10, 50, 256, 256), texture);
        GUILayout.Space(264);

        GUILayout.BeginScrollView(scrollPosition);
        TextureSettings();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Rebuild", GUILayout.MaxWidth(150)))
        {
            Rebuild();
        }
        if(GUILayout.Button("Save", GUILayout.MaxWidth(150)))
        {
            string path = EditorUtility.SaveFilePanel("Save...", "Assets", "texture", "png");
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.Refresh();
            //string normalPath = path.Replace(".png", "_N.png");
            //System.IO.File.WriteAllBytes(normalPath, GetNormal().EncodeToPNG());
        }
        GUILayout.EndHorizontal();
        EditorGUI.BeginChangeCheck();
        offset = EditorGUILayout.Vector2Field("Offset", offset);
        rotation = EditorGUILayout.Slider(rotation, 0, 360);
        fillColor = EditorGUILayout.ColorField(fillColor);
        secondary = EditorGUILayout.ColorField(secondary);
        if (EditorGUI.EndChangeCheck() || NoiseBoxWindow.NoiseBoxEditor(noiseBox))
        {
            Rebuild();
        }
        GUILayout.EndScrollView();
    }

    void TextureSettings()
    {
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        width = EditorGUILayout.IntField("Width:", width);
        height = EditorGUILayout.IntField("Height", height);
        if(EditorGUI.EndChangeCheck())
        {
            texture = new Texture2D(width, height);
            Rebuild();
        }
        GUILayout.EndHorizontal();
    }

    void Rebuild()
    {
        for(int i=0; i < width; i++)
        {
            for(int j=0; j < height; j++)
            {
                float xCoord = (i / (float)width) + offset.x;
                float yCoord = (j / (float)height) + offset.y;
                Vector2 r = Quaternion.AngleAxis(rotation, Vector3.forward)*new Vector2(xCoord, yCoord);

                if(noiseBox.colored)
                {
                    Color c = noiseBox.ResolveAsColor(r.x, r.y);
                    texture.SetPixel(i, j, c);
                }
                else
                {
                    float n = noiseBox.Resolve(r.x, r.y);
                    texture.SetPixel(i, j, n*fillColor+(1-n)*secondary);
                }
            }
        }
        texture.Apply();
    }

    Texture2D GetNormal()
    {
        Texture2D normal = new Texture2D(texture.width, texture.height);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float xLeft = texture.GetPixel(x - 1, y).grayscale;
                float xRight = texture.GetPixel(x + 1, y).grayscale;
                float yUp = texture.GetPixel(x, y - 1).grayscale;
                float yDown = texture.GetPixel(x, y + 1).grayscale;
                float xDelta = ((xLeft - xRight) + 1) * 0.5f;
                float yDelta = ((yUp - yDown) + 1) * 0.5f;

                normal.SetPixel(x, y, new Color(xDelta, yDelta, 1.0f, 1.0f));
            }
        }
        return normal;
    }
}