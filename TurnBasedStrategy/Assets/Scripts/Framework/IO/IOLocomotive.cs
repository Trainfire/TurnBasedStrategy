using UnityEngine;
using Framework.Components;

namespace Framework.IO
{
    public class IOLocomotive : MonoBehaviour
    {
        [SerializeField]
        private bool _startOn;

        [SerializeField]
        private Locomotive _locomotive;

        [SerializeField]
        private Orientation _initialOrientation;

        void Start()
        {
            if (_startOn && _locomotive != null)
                _locomotive.Move(_initialOrientation);
        }
    }
}
