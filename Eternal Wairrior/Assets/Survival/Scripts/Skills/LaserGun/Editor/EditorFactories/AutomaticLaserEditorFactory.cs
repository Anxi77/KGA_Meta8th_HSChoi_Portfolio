using EditorWrapper;
using UnityEditor;

namespace LaserSystem2D
{
    public class AutomaticLaserEditorFactory : IEditorFactory<AutomaticLaser>
    {
        public IDrawable Create(SerializedObject serializedObject, AutomaticLaser target)
        {
            Property infiniteWorking = new(serializedObject.FindProperty("_infiniteWorking"));
            DrawableComposite loopedLaserData = new(new IDrawable[]
            {
                new Property(serializedObject.FindProperty("_startWithSleepMode")),
                new Property(serializedObject.FindProperty("_waitTimeAtStart")),
                new Property(serializedObject.FindProperty("_workTime")),
                new Property(serializedObject.FindProperty("_sleepTime")),
            });
            
            return new DrawableComposite(new IDrawable[]
            {
                infiniteWorking,
                new DrawableIf(loopedLaserData, ()=> target.InfiniteWorking == false)
            });
        }
    }
}