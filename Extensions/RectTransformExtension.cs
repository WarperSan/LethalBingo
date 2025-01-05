using UnityEngine;

namespace LethalBingo.Extensions;

public static class RectTransformExtension
{
    public static void FillParent(this RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}