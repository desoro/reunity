using UnityEngine;

namespace Phuntasia
{
    public static class UnityExtensions
    {
        public static string ToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static void SetParentAndZero(this Transform child, Transform parent)
        {
            child.SetParent(parent, false);

            child.localPosition = Vector3.zero;
            child.localEulerAngles = Vector3.zero;
        }
    }
}