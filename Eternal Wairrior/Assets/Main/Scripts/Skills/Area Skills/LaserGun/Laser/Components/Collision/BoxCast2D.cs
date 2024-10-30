using System;
using System.Collections.Generic;
using UnityEngine;

namespace LaserSystem2D
{
    public class BoxCast2D
    {
        private readonly Dictionary<Collider2D, List<RaycastHit2D>> _result = new();
        private readonly RaycastHit2D[] _hits = new RaycastHit2D[15];

        public Dictionary<Collider2D, List<RaycastHit2D>> Update(BoxCastData boxCastData)
        {
            return Update(stackalloc BoxCastData[] { boxCastData }, 1);
        }

        public Dictionary<Collider2D, List<RaycastHit2D>> Update(in ReadOnlySpan<BoxCastData> boxCastData, int count)
        {
            _result.Clear();

            for (int i = 0; i < count; ++i)
            {
                BoxCast(in boxCastData[i]);
            }

            return _result;
        }

        private void BoxCast(in BoxCastData data)
        {
            int hits = Utils.BoxCast(data, _hits);

            for (int i = 0; i < hits; ++i)
            {
                _result.TryAdd(_hits[i].collider, new List<RaycastHit2D>());
                _result[_hits[i].collider].Add(_hits[i]);
            }
        }
    }
}