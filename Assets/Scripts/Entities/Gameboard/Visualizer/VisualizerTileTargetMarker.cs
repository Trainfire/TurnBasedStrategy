using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class VisualizerTileTargetMarker : MonoBehaviour, IStateVisualizer
{
    [SerializeField] private GameObject _view;

    private Tile _tile;

    public void Initialize(IStateEvents stateEvents)
    {
        stateEvents.PreviewAttackTarget += OnPreviewAttackTarget;

        _tile = gameObject.GetComponent<Tile>();

        Assert.IsNotNull(_view);
        Assert.IsNotNull(_tile);

        _view.SetActive(false);
        _view.transform.SetParent(_tile.transform);
    }

    private void OnPreviewAttackTarget(Tile tile)
    {
        if (_tile == tile)
        {
            StartCoroutine(PostPreview());
        }
    }

    private IEnumerator PostPreview()
    {
        _view.SetActive(true);

        yield return new WaitForSeconds(1f);

        _view.SetActive(false);
    }
}
