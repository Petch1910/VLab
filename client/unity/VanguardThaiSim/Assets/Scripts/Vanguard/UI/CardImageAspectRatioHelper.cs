using UnityEngine;

namespace VanguardThaiSim.UI
{
    public static class CardImageAspectRatioHelper
    {
        public const float DefaultCardAspectRatio = 0.714f;
        public const float MinimumAspectRatio = 0.55f;
        public const float MaximumAspectRatio = 0.85f;

        public static float Resolve(Texture2D texture)
        {
            if (texture == null || texture.height <= 0)
            {
                return DefaultCardAspectRatio;
            }

            float ratio = (float)texture.width / texture.height;
            return Mathf.Clamp(ratio, MinimumAspectRatio, MaximumAspectRatio);
        }
    }
}
