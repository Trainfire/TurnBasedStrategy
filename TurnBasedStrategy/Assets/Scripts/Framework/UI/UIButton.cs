using System;
using UnityEngine;
using UnityEngine.UI;
using Framework;

namespace Framework.UI
{
    [RequireComponent(typeof(Button))]
    public abstract class UIButton : MonoBehaviour
    {
        public event Action<UIButton> Pressed;

        [SerializeField] private Text _label;
        [SerializeField] private Button _button;

        public Button Button
        {
            get { return _button; }
        }

        public string Label
        {
            set
            {
                if (_label == null)
                {
                    Debug.LogError("Label is null as cannot be assigned a value.");
                }
                else
                {
                    _label.text = value;
                }
            }
        }

        public virtual void Awake()
        {
            _button.onClick.AddListener(OnPress);
        }

        public virtual void Selected(bool selected)
        {

        }

        protected virtual void OnPress()
        {
            if (Pressed != null)
                Pressed.Invoke(this);
        }
    }
}
