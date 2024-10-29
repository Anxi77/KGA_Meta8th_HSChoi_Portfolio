using UnityEngine;

namespace LaserSystem2D
{
    public class LaserComponents
    {
        public readonly DissolveShutdown DissolveShutdown;
        public readonly TransformMapper TransformMapper;
        public readonly LineRebuilding LineRebuilding;
        public readonly LaserInteraction Interaction;
        public readonly LaserActiveHits ActiveHits;
        public readonly LaserCollision Collision;
        public readonly RaycastHitEvent HitEvent;
        public readonly LaserRaycast Raycast;
        public readonly HitEffect HitEffect;
        public readonly LaserLength Length;
        public readonly LaserAudio Audio;
        public readonly LaserView View;

        public LaserComponents(LaserData data, LaserDissolve dissolve, LaserBase laserBase)
        {
            MeshRenderer meshRenderer = laserBase.GetComponent<MeshRenderer>();
            Interaction = new LaserInteraction(laserBase);
            Raycast = new LaserRaycast(laserBase.transform, data.RaycastData, data.Line);
            Length = new RaycastLaserLength(data.RaycastData, data.Line, Raycast);
            ActiveHits = new LaserActiveHits(Length, Raycast);
            HitEffect = new RaycastHitEffect(data.HitEffectPrefab, ActiveHits, Raycast, data.ViewData);
            TransformMapper = new TransformMapper(laserBase.transform);
            View = new LaserView(meshRenderer, data.Line, dissolve, Length, data.ViewData);
            Collision = new LaserCollision(data.CollisionData, Length, Raycast);
            HitEvent = new RaycastHitEvent(ActiveHits, Raycast);
            Audio = new LaserAudio(data.LaserAudioSource, data.HitAudioSource, HitEvent);
            LineRebuilding = new LineRebuilding(data.Line, Raycast);
            DissolveShutdown = new DissolveShutdown(laserBase, dissolve, Length);
        }
    }
}