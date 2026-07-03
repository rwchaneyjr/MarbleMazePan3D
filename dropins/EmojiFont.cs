using UnityEngine;

namespace DragonBoxAlgebra.UI
{
    public static class EmojiFont
    {
        private static Font _font;

        public static Font Get()
        {
            if (_font != null)
            {
                return _font;
            }

            string[] names =
            {
                "Segoe UI Emoji",
                "Apple Color Emoji",
                "Noto Color Emoji",
                "Arial"
            };

            foreach (string name in names)
            {
                _font = Font.CreateDynamicFontFromOSFont(name, 48);
                if (_font != null)
                {
                    return _font;
                }
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
