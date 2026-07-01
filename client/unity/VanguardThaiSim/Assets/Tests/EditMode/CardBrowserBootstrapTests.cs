using System.Reflection;
using NUnit.Framework;
using VanguardThaiSim.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class CardBrowserBootstrapTests
    {
        [TearDown]
        public void Cleanup()
        {
            CardBrowserBootstrap[] browsers =
                Object.FindObjectsByType<CardBrowserBootstrap>(FindObjectsInactive.Include);
            foreach (CardBrowserBootstrap browser in browsers)
            {
                Object.DestroyImmediate(browser.gameObject);
            }

            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem != null)
            {
                Object.DestroyImmediate(eventSystem.gameObject);
            }
        }

        [Test]
        public void DeckBuilderScreenShowsPlayerFacingReadinessSummary()
        {
            GameObject host = new GameObject("Card Browser Bootstrap Readiness Test");
            CardBrowserBootstrap bootstrap = host.AddComponent<CardBrowserBootstrap>();

            typeof(CardBrowserBootstrap)
                .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(bootstrap, null);

            GameObject summaryObject = FindChild(host.transform, "Card Workshop Summary Text");
            Assert.NotNull(summaryObject);

            Text summary = summaryObject.GetComponent<Text>();
            Assert.NotNull(summary);
            Assert.IsTrue(summary.text.Contains("Deck Builder ready"));
            Assert.IsTrue(summary.text.Contains("Showing"));
            Assert.IsTrue(summary.text.Contains("Filters:"));
            Assert.IsTrue(summary.text.Contains("add it to Main or Ride"));

            Text toolbarStatus = FindChild(host.transform, "Card Workshop Toolbar Status Text").GetComponent<Text>();
            Assert.NotNull(toolbarStatus);
            Assert.IsTrue(toolbarStatus.text.Contains("Cards "));
            Assert.IsTrue(toolbarStatus.text.Contains("Shown "));
            Assert.IsFalse(toolbarStatus.text.Contains("Pack validation:"));

            Image cacheButtonImage = FindChild(host.transform, "Cache Button").GetComponent<Image>();
            Assert.NotNull(cacheButtonImage);
            Assert.Less(cacheButtonImage.color.r, 0.4f);
            Assert.Less(cacheButtonImage.color.g, 0.4f);

            AspectRatioFitter detailImageAspect = FindChild(host.transform, "Card Detail Image").GetComponent<AspectRatioFitter>();
            Assert.NotNull(detailImageAspect);
            Assert.AreEqual(AspectRatioFitter.AspectMode.FitInParent, detailImageAspect.aspectMode);
            Assert.GreaterOrEqual(detailImageAspect.aspectRatio, CardImageAspectRatioHelper.MinimumAspectRatio);
            Assert.LessOrEqual(detailImageAspect.aspectRatio, CardImageAspectRatioHelper.MaximumAspectRatio);
        }

        private static GameObject FindChild(Transform root, string name)
        {
            if (root.name == name)
            {
                return root.gameObject;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                GameObject found = FindChild(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
