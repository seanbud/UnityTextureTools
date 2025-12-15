using UnityEngine;
using UnityEditor;

namespace Seanbud.TextureTools.Editor
{
    /// <summary>
    /// SpriteSetupHelper:
    /// Right-click any texture asset, choose "Set Sprite Single",
    /// automatically sets Sprite (2D and UI), Single mode, Full Rect,
    /// and disables the physics shape for quick, consistent sprite imports.
    /// </summary>
    public static class SetupAsSpriteSingle
    {
        // This attribute places the option in the top-right "gear" menu
        // on the TextureImporter's inspector.
        [MenuItem("CONTEXT/TextureImporter/Set Sprite Single")]
        private static void SetSpriteSingle(MenuCommand command)
        {
            var importer = command.context as TextureImporter;
            if (importer == null)
            {
                return;
            }

            // Optional Undo
            Undo.RegisterCompleteObjectUndo(importer, "Set Sprite Single");

            // Set basic sprite settings
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;

            // Set advanced sprite settings
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);

            // Apply
            importer.SaveAndReimport();
        }
    }
}
