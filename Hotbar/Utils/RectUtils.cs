using UnityEngine;

namespace Hotbar.Utils
{
    internal static class RectUtils
    {
        internal static Rect Pad(this Rect rect, float pad)
        {
            return new Rect(rect.x + pad, rect.y + pad, rect.width - pad, rect.height - pad);
        }

        internal static Rect Pad(this Rect rect, float w, float h)
        {
            return new Rect(rect.x + w, rect.y + h, rect.width - w, rect.height - h);
        }

        internal static Rect Pad(this Rect rect, float x, float y, float w, float h)
        {
            return new Rect(rect.x + x, rect.y + y, rect.width - w, rect.height - h);
        }
    }
}
