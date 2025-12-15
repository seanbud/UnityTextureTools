using UnityEngine;
using UnityEditor;
using System.IO;

namespace Seanbud.TextureTools.Editor
{
    public static class BlurTextureMenu
    {
        [MenuItem("CONTEXT/TextureImporter/Apply Gaussian Blur...")]
        private static void ShowBlurInput(MenuCommand command)
        {
            TextureImporter importer = command.context as TextureImporter;
            if (importer == null) return;

            BlurInputWindow.Open(importer);
        }

        private class BlurInputWindow : EditorWindow
        {
            private TextureImporter importer;
            private string radiusStr = "5";

            public static void Open(TextureImporter importer)
            {
                var window = ScriptableObject.CreateInstance<BlurInputWindow>();
                window.importer = importer;
                window.titleContent = new GUIContent("Blur Radius");
                window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 250, 80);
                window.ShowUtility();
            }

            void OnGUI()
            {
                GUILayout.Label("Enter Gaussian Blur Radius (px):", EditorStyles.boldLabel);
                radiusStr = EditorGUILayout.TextField("Radius", radiusStr);

                GUILayout.Space(10);
                if (GUILayout.Button("Apply"))
                {
                    if (int.TryParse(radiusStr, out int radius))
                    {
                        ApplyBlur(importer, radius);
                        Close();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid input", "Please enter a valid number.", "OK");
                    }
                }
            }

            private void ApplyBlur(TextureImporter importer, int radius)
            {
                string path = importer.assetPath;
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex == null) return;

                Texture2D readableTex = GetReadableCopy(tex);
                Texture2D blurred = GaussianBlur(readableTex, radius);

                byte[] pngData = blurred.EncodeToPNG();
                string newPath = Path.Combine(Path.GetDirectoryName(path), tex.name + $"_blur_{radius}px.png");
                File.WriteAllBytes(newPath, pngData);
                AssetDatabase.Refresh();
            }

            private Texture2D GetReadableCopy(Texture2D source)
            {
                RenderTexture tmp = RenderTexture.GetTemporary(source.width, source.height);
                Graphics.Blit(source, tmp);
                RenderTexture.active = tmp;
                Texture2D readable = new Texture2D(source.width, source.height);
                readable.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                readable.Apply();
                RenderTexture.ReleaseTemporary(tmp);
                return readable;
            }

            private Texture2D GaussianBlur(Texture2D source, int radius)
            {
                int w = source.width, h = source.height;
                Texture2D result = new Texture2D(w, h);
                Color[] pix = source.GetPixels();

                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        Color sum = Color.black;
                        int count = 0;
                        for (int dy = -radius; dy <= radius; dy++)
                            for (int dx = -radius; dx <= radius; dx++)
                            {
                                int nx = Mathf.Clamp(x + dx, 0, w - 1);
                                int ny = Mathf.Clamp(y + dy, 0, h - 1);
                                sum += pix[ny * w + nx];
                                count++;
                            }
                        result.SetPixel(x, y, sum / count);
                    }

                result.Apply();
                return result;
            }
        }
    }
}
