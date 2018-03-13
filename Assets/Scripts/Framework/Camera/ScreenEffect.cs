using UnityEngine;
using System;

namespace Framework.Components
{
    public abstract class ScreenEffect : MonoBehaviour
    {
        public event Action<ScreenEffect> Finished;

        public abstract void ProcessEffect();
        public abstract void Activate();
        public abstract void Deactivate();

        protected virtual void OnFinish()
        {
            if (Finished != null)
                Finished(this);
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }
    }
}
