using UnityEngine;

namespace vrcode.browser
{
    public struct CursorInput
    {
        public bool BackPage;
        public bool ForwardPage;

        public bool LeftClick;
        public bool RightClick;
        // bool MiddleClick; going to make a judgement call here and say this one doesn't matter

        public bool MouseHasFocus;

        public Vector2 MousePosition;

        public Vector2 MouseScroll;
    }
}