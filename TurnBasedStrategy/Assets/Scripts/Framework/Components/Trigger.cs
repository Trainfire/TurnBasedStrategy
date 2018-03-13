using UnityEngine;
using System;
using System.Collections.Generic;

namespace Framework
{
    /// <summary>
    /// Generic Trigger class.
    /// </summary>
    public class Trigger : MonoBehaviour
    {
        public event Action<Trigger> Triggered;

        public void Fire()
        {
            Triggered.InvokeSafe(this);
        }
    }
}
