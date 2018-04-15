using UnityEngine;
using UnityEngine.UI;

public class UIFillSegment : MonoBehaviour
{
    public bool Filled { get; set; }
    public bool Flashing { get; set; }

    [SerializeField] private float _flashSpeed;
    [SerializeField] private Color _fillColor;
    [SerializeField] private Image _fillImage;

    private void Update()
    {
        _fillImage.enabled = Filled;

        if (_fillImage.enabled)
        {
            var color = _fillImage.color;

            if (Flashing)
            {
                color.a = Mathf.Lerp(0f, 1f, Mathf.Abs(Mathf.Sin(Time.time / _flashSpeed)));
            }
            else
            {
                color.a = _fillColor.a;
            }

            _fillImage.color = color;
        }
    }
}
