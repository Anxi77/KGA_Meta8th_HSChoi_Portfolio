using UnityEngine;

namespace LaserSystem2D
{
    public class MouseState
    {
        public bool IsHolding { get; private set; }
        public bool IsMouseDown { get; private set; }
        public bool IsMouseUp { get; private set; }
        
        public void Update()
        {
            IsMouseDown = CheckMouseState(EventType.MouseDown);
            IsMouseUp = CheckMouseState(EventType.MouseUp);
            
            if (IsMouseDown)
            {
                IsHolding = true;
            }
            
            if (IsMouseUp)
            {
                IsHolding = false;
            }
        }

        private bool CheckMouseState(EventType eventType, int button = 0)
        {
            return Event.current.type == eventType &&
                   Event.current.button == button &&
                   Event.current.modifiers == EventModifiers.None;
        }

        public bool CheckRightClick()
        {
            return CheckMouseState(EventType.MouseDown, 1);
        }
    }
}