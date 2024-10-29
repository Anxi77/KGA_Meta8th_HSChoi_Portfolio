using EditorWrapper;
using UnityEditor;

namespace LaserSystem2D
{
    public abstract class BaseLaserEditorFactory : IEditorFactory<LaserBase>
    {
        protected readonly int Space = 10;
        private SerializedProperty _data;
        protected SerializedObject SerializedObject { get; private set; }

        protected abstract IDrawable CreateFirstElement();
        protected virtual IDrawable EnableNonHitEffect { get; } = new DrawableDummy(); 
        protected virtual IDrawable MaxPoints { get; } = new DrawableDummy();

        public IDrawable Create(SerializedObject serializedObject, LaserBase laserBase)
        {
            SerializedObject = serializedObject;
            _data = serializedObject.FindProperty("_data");
            
            return new DrawableComposite(new[]
            {
                CreateFirstElement(),
                CreateCollisionSection(),
                CreateLineSection(),
                CreateViewSection(),
                CreateSoundSection(),
            });
        }

        private IDrawable CreateCollisionSection()
        {
            return new Section("Collision", Space, new IDrawable[]
            {
                new Property(FindPropertyInData("_collisionData", "_mask")),
                new Property(FindPropertyInData("_collisionData", "_width")),
                new Property(FindPropertyInData("_collisionData", "_penetration")),
            });
        }

        private IDrawable CreateLineSection()
        {
            return new Section("Line", Space, new IDrawable[]
            {
                MaxPoints,
                new Property(FindPropertyInData("_line", "_width")),
            });
        }
        
        private IDrawable CreateViewSection()
        {
            return new Section("View", Space, new IDrawable[]
            {
                EnableNonHitEffect,
                new Property(FindPropertyInData("_viewData", "_hitEffectPrefab")),
                new Property(FindPropertyInData("_viewData", "_dissolveTime")),
                new Property(FindPropertyInData("_viewData", "_sortingOrder")),
            });
        }
        
        private IDrawable CreateSoundSection()
        {
            return new Section("Audio", 0, new IDrawable[]
            {
                new Property(FindPropertyInData("_hitAudioSource")),
                new Property(FindPropertyInData("_laserAudioSource")),
            });
        }

        protected SerializedProperty FindPropertyInData(params string[] tree)
        {
            return _data.FindPropertyRelative(tree);
        }
    }
}