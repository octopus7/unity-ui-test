using UnityEngine;
using UnityEditor;

public class ItemTexturePostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // Check if the asset is in the specific folder
        if (assetPath.Contains("Assets/Textures/Items"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            
            // Only set if not already set (optional, but good for first import)
            // Or force it every time to ensure consistency
            
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; // UI sprites usually don't need mipmaps
            
            // Optional: compression settings for high quality UI
            TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
            settings.name = "Standalone";
            settings.overridden = true;
            settings.format = TextureImporterFormat.RGBA32; // High quality w/ alpha
            importer.SetPlatformTextureSettings(settings);
            
            Debug.Log($"[ItemTexturePostprocessor] Automatically configured UI sprite settings for: {assetPath}");
        }
    }
}
