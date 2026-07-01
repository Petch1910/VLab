using NUnit.Framework;
using VanguardThaiSim.UI;
using UnityEngine;

namespace VanguardThaiSim.Tests
{
    public sealed class CardImageAspectRatioHelperTests
    {
        [Test]
        public void NullTextureUsesDefaultCardRatio()
        {
            Assert.AreEqual(
                CardImageAspectRatioHelper.DefaultCardAspectRatio,
                CardImageAspectRatioHelper.Resolve(null));
        }

        [Test]
        public void PortraitTextureUsesWidthOverHeight()
        {
            Texture2D texture = new Texture2D(771, 1088);
            try
            {
                Assert.AreEqual(771f / 1088f, CardImageAspectRatioHelper.Resolve(texture), 0.001f);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void ExtremeTextureRatiosAreClampedForCardPreview()
        {
            Texture2D wide = new Texture2D(1200, 200);
            Texture2D narrow = new Texture2D(100, 1000);
            try
            {
                Assert.AreEqual(CardImageAspectRatioHelper.MaximumAspectRatio, CardImageAspectRatioHelper.Resolve(wide));
                Assert.AreEqual(CardImageAspectRatioHelper.MinimumAspectRatio, CardImageAspectRatioHelper.Resolve(narrow));
            }
            finally
            {
                Object.DestroyImmediate(wide);
                Object.DestroyImmediate(narrow);
            }
        }
    }
}
