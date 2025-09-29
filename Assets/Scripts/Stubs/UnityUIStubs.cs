#if !UNITY_5_3_OR_NEWER
using UnityEngine;

namespace UnityEngine.UI
{
    public class Graphic : MonoBehaviour
    {
    }

    public class Text : Graphic
    {
        public Font font { get; set; }
        public TextAnchor alignment { get; set; }
        public bool supportRichText { get; set; }
        public HorizontalWrapMode horizontalOverflow { get; set; }
        public VerticalWrapMode verticalOverflow { get; set; }
        public string text { get; set; }
    }
}
#endif
