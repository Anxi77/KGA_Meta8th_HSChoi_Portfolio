using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Collections.Generic;

public class ResourceManager<T> : IDataManager<T> where T : UnityEngine.Object
{
    private readonly string basePath;
    private readonly Dictionary<string, T> cache;

    public ResourceManager(string basePath)
    {
        this.basePath = basePath;
        this.cache = new Dictionary<string, T>();
    }

    public void SaveData(string key, T data)
    {
        if (data == null) return;

        try
        {
            // 텍스처 경로 생성
            string fullPath = Path.Combine(Application.dataPath, "Resources", basePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                Debug.Log($"Created directory: {fullPath}");
            }

            string assetPath = $"Assets/Resources/{basePath}/{key}";
#if UNITY_EDITOR
            // 스프라이트인지 게임오브젝트인지 확인
            if (data is Sprite sprite)
            {
                SaveSprite(assetPath, sprite);
            }
            else if (data is GameObject prefab)
            {
                SavePrefab(assetPath, prefab);
            }

            AssetDatabase.Refresh();
#endif
            cache[key] = data;

            Debug.Log($"Saved resource to: {assetPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving resource: {e.Message}\n{e.StackTrace}");
        }
    }
    public T LoadData(string key)
    {
        if (cache.TryGetValue(key, out T cachedData))
            return cachedData;

        string resourcePath = Path.Combine(basePath, key);
        T data = Resources.Load<T>(resourcePath);

        if (data != null)
            cache[key] = data;

        return data;
    }

    public bool DeleteData(string key)
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, "Resources", basePath, key);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                cache.Remove(key);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error deleting resource: {e.Message}");
        }
        return false;
    }

    public void ClearAll()
    {
        try
        {
            string directory = Path.Combine(Application.dataPath, "Resources", basePath);
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
                Directory.CreateDirectory(directory);
            }
            cache.Clear();
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error clearing resources: {e.Message}");
        }
    }
#if UNITY_EDITOR
    private void SaveSprite(string path, Sprite sprite)
    {
        try
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fullPath = $"{path}.png";
            // 텍스처 원본 경로 가져오기
            string texturePath = AssetDatabase.GetAssetPath(sprite.texture);

            // 원본 파일이 존재하면 직접 복사
            if (File.Exists(texturePath))
            {
                File.Copy(texturePath, fullPath, true);
                Debug.Log($"Copied sprite from {texturePath} to {fullPath}");
            }
            else
            {
                // 원본 파일이 없는 경우에만 텍스처 변환 수행
                TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                if (importer != null)
                {
                    bool originalReadable = importer.isReadable;
                    TextureImporterCompression originalCompression = importer.textureCompression;

                    try
                    {
                        importer.isReadable = true;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();

                        // 스프라이트의 실제 크기로 새 텍스처 생성
                        Rect spriteRect = sprite.rect;
                        Texture2D tempTexture = new Texture2D(
                            (int)spriteRect.width,
                            (int)spriteRect.height,
                            TextureFormat.RGBA32,
                            false);

                        // 스프라이트의 픽셀 데이터 직접 복사
                        var pixels = sprite.texture.GetPixels(
                            (int)spriteRect.x,
                            (int)spriteRect.y,
                            (int)spriteRect.width,
                            (int)spriteRect.height);
                        tempTexture.SetPixels(pixels);
                        tempTexture.Apply();

                        byte[] bytes = tempTexture.EncodeToPNG();
                        if (bytes != null && bytes.Length > 0)
                        {
                            File.WriteAllBytes(fullPath, bytes);
                            Debug.Log($"Saved sprite to: {fullPath}");
                        }
                        else
                        {
                            Debug.LogError("Failed to encode texture to PNG");
                        }

                        Object.DestroyImmediate(tempTexture);
                    }
                    finally
                    {
                        // 원래 설정 복구
                        importer.isReadable = originalReadable;
                        importer.textureCompression = originalCompression;
                        importer.SaveAndReimport();
                    }
                }
                else
                {
                    Debug.LogError($"Could not get TextureImporter for sprite: {texturePath}");
                }
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving sprite: {e.Message}\n{e.StackTrace}");
        }
    }

    private void SavePrefab(string path, GameObject prefab)
    {
        try
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string fullPath = $"{path}.prefab";

            // 프리팹 파일이 존재하면 삭제
            if (File.Exists(fullPath))
            {
                AssetDatabase.DeleteAsset(fullPath);
            }

            // 프리팹 인스턴스 생성
            GameObject prefabInstance = Object.Instantiate(prefab);
            bool success = PrefabUtility.SaveAsPrefabAsset(prefabInstance, fullPath, out bool prefabSuccess);
            Object.DestroyImmediate(prefabInstance);

            if (success)
            {
                Debug.Log($"Saved prefab to: {fullPath}");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"Failed to save prefab to: {fullPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving prefab: {e.Message}");
        }
    }
#endif
}
