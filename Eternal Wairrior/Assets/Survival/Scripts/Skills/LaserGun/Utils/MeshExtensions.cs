using UnityEngine;

namespace LaserSystem2D
{
    public class MeshExtensions
    {
        public static Mesh CreateQuad(float width, float height)
        {
            return new Mesh()
            {
                vertices = new Vector3[4]
                {
                    new(0 - width / 2, -height / 2, 0),
                    new(width - width / 2, -height / 2, 0),
                    new(0 - width / 2, height - height / 2, 0),
                    new(width - width / 2, height - height / 2, 0)
                },
                triangles = new int[6]
                {
                    0, 2, 1,
                    2, 3, 1
                },
                uv = new Vector2[4]
                {
                    new(0, 0),
                    new(1, 0),
                    new(0, 1),
                    new(1, 1)
                }
            };
        }
    }
}