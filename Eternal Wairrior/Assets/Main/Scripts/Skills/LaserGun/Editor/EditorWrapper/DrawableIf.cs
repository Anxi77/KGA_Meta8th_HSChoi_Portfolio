using System;

namespace EditorWrapper
{ 
    public class DrawableIf : IDrawable
    {
        private readonly IDrawable _conditionalDrawable;
        
        public DrawableIf(IDrawable drawable, Func<bool> drawCondition)
        {
            _conditionalDrawable = new ConditionalDraw(drawable, new DrawableDummy(), drawCondition);
        }
        
        public void Draw()
        {
            _conditionalDrawable.Draw();
        }
    }
}