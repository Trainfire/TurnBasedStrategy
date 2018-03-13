using UnityEngine;

namespace Framework.Components
{
    public enum Orientation
    {
        Forwards,
        Backwards,
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class Locomotive : MonoBehaviour
    {
        [SerializeField]
        private PathTrack _path;
        [SerializeField]
        private bool _loop;
        [SerializeField]
        private float _moveSpeed;

        private Rigidbody2D _rigidBody;
        private float _distanceTravelled;
        private float _targetDistance;
        private Vector3 _target;
        private Vector3 _direction;
        private int _index;

        private Orientation _orientation;

        private State _state;
        enum State
        {
            Idle,
            Moving
        }

        void Awake()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
            _rigidBody.isKinematic = true;

            if (_path == null)
            {
                Debug.LogWarning("No path has been assigned.", this);
            }
            else
            {
                transform.position = _path.Points[0];
            }
        }

        public void Move(Orientation orientation)
        {
            // TODO: Handle orientation checks.

            if (!_path.HasPoints())
            {
                Debug.LogWarning("Lift cannot move as no points are available.");
                return;
            }

            _orientation = orientation;
            MoveNext();
        }

        void Update()
        {
            if (_state == State.Idle)
                return;

            float moveSpeed = _moveSpeed * Time.deltaTime;
            _rigidBody.MovePosition(transform.position += _direction * moveSpeed);
            _distanceTravelled += moveSpeed;

            if (_distanceTravelled >= _targetDistance)
            {
                transform.position = _target;
                MoveNext();
            }
        }

        void MoveNext()
        {
            //if (_orientation == Orientation.Forwards)
            //{
            //    if (!_path.Exists(_index + 1))
            //    {
            //        if (_loop)
            //        {
            //            _orientation = Orientation.Backwards;
            //            _index--;
            //        }
            //        else
            //        {
            //            _state = State.Idle;
            //        }
            //    }
            //    else
            //    {
            //        _index++;
            //    }
            //}
            //else
            //{
            //    if (!_path.Exists(_index - 1))
            //    {
            //        if (_loop)
            //        {
            //            _orientation = Orientation.Forwards;
            //            _index++;
            //        }
            //        else
            //        {
            //            _state = State.Idle;
            //        }
            //    }
            //    else
            //    {
            //        _index--;
            //    }
            //}

            var next = _path.Next(_index, _orientation);
            if (next != null)
            {
                _index = next.Index;
                _target = next.Position;
                _direction = (_target - transform.position).normalized;
                _targetDistance = Vector3.Distance(transform.position, _target);
                _distanceTravelled = 0f;

                _state = State.Moving;
            }
            else
            {
                _state = State.Idle;
            }
        }
    }
}
