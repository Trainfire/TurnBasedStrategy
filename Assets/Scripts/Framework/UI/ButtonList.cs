using UnityEngine;

namespace Framework.UI
{
    public class ButtonList : MonoBehaviour
    {
        [SerializeField] private UIButton _prototype;

        private CyclicalList<UIButton> _buttons;
        private UIButton _last;

        private void Awake()
        {
            _buttons = new CyclicalList<UIButton>();
            _buttons.Wrapped = true;
            _buttons.Moved += OnMove;

            _prototype.gameObject.SetActive(false);
        }

        private void OnMove(object sender, CyclicalListEvent<UIButton> cycleEvent)
        {
            _last.Selected(false);
            cycleEvent.Data.Selected(true);
            _last = cycleEvent.Data;
        }

        public UIButton Add(string label)
        {
            var instance = UIUtility.Add<UIButton>(transform, _prototype.gameObject);
            instance.Label = label;

            if (_last == null)
            {
                _last = instance;
                _last.Selected(true);
            }

            _buttons.Add(instance);

            return instance;
        }

        public void Prev()
        {
            _buttons.MovePrev();
        }

        public void Next()
        {
            _buttons.MoveNext();
        }

        public void Update()
        {
            if (_buttons.Current != null)
                _buttons.Current.Button.Select();
        }
    }
}
