using UnityEngine;
using Framework;
using System.Collections.Generic;

namespace Framework.Components
{
    public class PathNode
    {
        public int Index { get; private set; }
        public Vector3 Position { get; private set; }

        public PathNode(int index, Vector3 position)
        {
            Index = index;
            Position = position;
        }
    }

    public class PathTrack : MonoBehaviour
    {
        [SerializeField] private bool _looped;
        public bool Looped { get { return _looped; } }

        [SerializeField] private List<Vector3> _points;
        public List<Vector3> Points { get { return _points; } }

        public void SetPoints(List<Vector3> points)
        {
            _points = points;
        }

        public bool Exists(int index)
        {
            return _points.InRange(index);
        }

        public PathNode Next(int currentIndex, Orientation orientation)
        {
            int nextIndex = orientation == Orientation.Forwards ? currentIndex + 1 : currentIndex - 1;

            if (orientation == Orientation.Forwards)
            {
                if (nextIndex >= Points.Count)
                {
                    return _looped ? First() : null;
                }
                else
                {
                    return new PathNode(nextIndex, Points[nextIndex]);
                }
            }
            else if (orientation == Orientation.Backwards)
            {
                if (nextIndex < 0)
                {
                    return _looped ? Last() : null;
                }
                else
                {
                    return new PathNode(nextIndex, Points[nextIndex]);
                }
            }

            return null;
        }
    
        public void Add(int index)
        {
            if (index != Points.Count - 1)
            {
                // Insert
                Points.Insert(index + 1, Vector3.Lerp(Points[index], Points[index + 1], 0.5f));
            }
            else
            {
                // Add
                Points.Insert(index, Points[index]);
            }
        }

        public void Remove(int index)
        {
            Points.RemoveAt(index);
        }

        public bool HasPoints()
        {
            return _points.Count != 0;
        }

        public PathNode First()
        {
            if (_points.Count != 0)
                return new PathNode(0, _points[0]);
            return null;
        }

        public PathNode Last()
        {
            if (_points.Count > 1)
            {
                int index = _points.Count - 1;
                return new PathNode(index, _points[index]);
            }
            return null;
        }
    }
}
