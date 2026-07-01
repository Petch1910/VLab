using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VanguardThaiSim.Cards
{
    public sealed class CardImageCache : IDisposable
    {
        private readonly string imageRoot;
        private readonly int maxThumbnails;
        private readonly int maxFullImages;
        private readonly Dictionary<string, Texture2D> thumbnailCache = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, Texture2D> fullImageCache = new Dictionary<string, Texture2D>();
        private readonly Queue<string> thumbnailOrder = new Queue<string>();
        private readonly Queue<string> fullImageOrder = new Queue<string>();
        private Texture2D fallbackTexture;

        public int ThumbnailCount => thumbnailCache.Count;
        public int FullImageCount => fullImageCache.Count;

        public CardImageCache(string imageRoot, int maxThumbnails = 96, int maxFullImages = 4)
        {
            if (string.IsNullOrEmpty(imageRoot))
            {
                throw new ArgumentException("Image root is required.", nameof(imageRoot));
            }

            this.imageRoot = imageRoot;
            this.maxThumbnails = Math.Max(1, maxThumbnails);
            this.maxFullImages = Math.Max(1, maxFullImages);
        }

        public Texture2D LoadThumbnail(string imageRelativePath)
        {
            return LoadCached(imageRelativePath, thumbnailCache, thumbnailOrder, maxThumbnails);
        }

        public Texture2D LoadFullImage(string imageRelativePath)
        {
            return LoadCached(imageRelativePath, fullImageCache, fullImageOrder, maxFullImages);
        }

        public Texture2D GetFallbackTexture()
        {
            if (fallbackTexture != null)
            {
                return fallbackTexture;
            }

            fallbackTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Color32[] pixels =
            {
                new Color32(48, 48, 52, 255),
                new Color32(72, 72, 78, 255),
                new Color32(72, 72, 78, 255),
                new Color32(48, 48, 52, 255)
            };
            fallbackTexture.SetPixels32(pixels);
            fallbackTexture.Apply(false, true);
            return fallbackTexture;
        }

        public bool IsFallbackTexture(Texture2D texture)
        {
            return texture != null && fallbackTexture != null && ReferenceEquals(texture, fallbackTexture);
        }

        public void ClearMemory()
        {
            DestroyCachedTextures(thumbnailCache);
            DestroyCachedTextures(fullImageCache);
            thumbnailOrder.Clear();
            fullImageOrder.Clear();
        }

        public void Dispose()
        {
            ClearMemory();
            if (fallbackTexture != null)
            {
                DestroyTexture(fallbackTexture);
                fallbackTexture = null;
            }
        }

        private Texture2D LoadCached(
            string imageRelativePath,
            Dictionary<string, Texture2D> cache,
            Queue<string> order,
            int maxCount)
        {
            if (string.IsNullOrEmpty(imageRelativePath))
            {
                return GetFallbackTexture();
            }

            string key = imageRelativePath.Replace('\\', '/');
            if (cache.TryGetValue(key, out Texture2D cached) && cached != null)
            {
                return cached;
            }

            Texture2D loaded = LoadTextureFromDisk(key);
            if (loaded == null)
            {
                return GetFallbackTexture();
            }

            cache[key] = loaded;
            order.Enqueue(key);
            TrimCache(cache, order, maxCount);
            return loaded;
        }

        private Texture2D LoadTextureFromDisk(string imageRelativePath)
        {
            string path = Path.Combine(imageRoot, imageRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] data = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!texture.LoadImage(data))
            {
                DestroyTexture(texture);
                return null;
            }

            return texture;
        }

        private static void TrimCache(Dictionary<string, Texture2D> cache, Queue<string> order, int maxCount)
        {
            while (cache.Count > maxCount && order.Count > 0)
            {
                string oldest = order.Dequeue();
                if (!cache.TryGetValue(oldest, out Texture2D texture))
                {
                    continue;
                }

                cache.Remove(oldest);
                DestroyTexture(texture);
            }
        }

        private static void DestroyCachedTextures(Dictionary<string, Texture2D> cache)
        {
            foreach (Texture2D texture in cache.Values)
            {
                if (texture != null)
                {
                    DestroyTexture(texture);
                }
            }

            cache.Clear();
        }

        private static void DestroyTexture(Texture2D texture)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
