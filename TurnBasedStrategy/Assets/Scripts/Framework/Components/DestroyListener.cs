using UnityEngine;
using System;

namespace Framework
{
    class DestroyListener : MonoBehaviour
    {
        public event Action<DestroyListener> Destroyed;

        private void OnDestroy()
        {
            Destroyed.InvokeSafe(this);
        }
    }
}
