﻿namespace RPGEngine2
{
    public abstract class UIElementBase : BaseObject
    {
        /// <summary>
        /// Used to represent the state of the mouse based on an element.
        /// </summary>
        public enum HoverState
        {
            None = 0,
            Enter = 1,
            Leave = 2,
            Stay = 3,
        }
        public HoverState CurrentHoverState;

        /// <summary>
        /// True when mouse is hovering above element.
        /// </summary>
        public bool Hovered
        {
            get
            {
                return CurrentHoverState == HoverState.Enter || CurrentHoverState == HoverState.Stay;
            }
        }

        /// <summary>
        /// As an alternative to using <c>Update</c> method for checking hover state, this method is called when <c>CurrentHoverState</c> changes (NOTE: Only once even while hoverstate is stay)
        /// </summary>
        public virtual void HoverUpdate()
        {
        }

        /// <summary>
        /// Helper method to draw a border on a one-dimensional char-array.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="width"></param>
        /// <param name="borderchar"></param>
        protected static void DrawBorder(char[] screen, int width, char borderchar)
        {
            if (screen is null)
                return;

            for (int i = 0; i < screen.Length; i++)
            {
                if (i % width == 0 || i <= width || screen.Length - i <= width || i % width == width - 1)
                {
                    screen[i] = borderchar;
                }
            }
        }
    }
}
