using UnityEngine;
using Framework;

public class Player : MonoBehaviour
{
    [SerializeField] private UnitData _unitData;

    private PlayerUnitPlacer _unitPlacer;
    private Unit _selectedUnit;
    private Tile _tileUnderMouse;
    private bool _inputEnabled;

    private void Awake()
    {
        _unitPlacer = gameObject.GetOrAddComponent<PlayerUnitPlacer>();
        _inputEnabled = true;
    }

    void Update()
    {
        _tileUnderMouse = GetTileUnderMouse();

        // Highlight tile under mouse
        if (_tileUnderMouse)
            _tileUnderMouse.Marker.SetPositive();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Selected Unit: " + (_selectedUnit != null ? _selectedUnit.name : "Null"));
        GUILayout.Label("Tile Under Mouse: " + (_tileUnderMouse != null ? _tileUnderMouse.transform.position.TransformToGridspace().ToString() : "Null"));
        GUILayout.EndVertical();
    }

    void LateUpdate()
    {
        if (!_inputEnabled)
            return;

        if (_unitPlacer.IsPlacingUnit)
        {
            if (Input.GetMouseButtonUp(1))
                _unitPlacer.CancelPlacingUnit();

            if (Input.GetMouseButtonDown(0))
                _unitPlacer.PlaceUnit(_tileUnderMouse);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.F))
                _unitPlacer.BeginPlacingUnit(_unitData);

            if (Input.GetMouseButtonDown(0))
            {
                var unitUnderMouse = GetUnitUnderMouse();

                // Deselect unit if unit is selected.
                if (_selectedUnit != null && unitUnderMouse == null)
                {
                    _selectedUnit = null;
                }
                else
                {
                    _selectedUnit = unitUnderMouse;
                }
            }

            // Move.
            if (Input.GetMouseButtonDown(1) && _tileUnderMouse != null && _selectedUnit != null)
                _selectedUnit.Move(_tileUnderMouse);
        }
    }

    static T GetComponentUnderMouse<T>() where T : MonoBehaviour
    {
        var mouseToScreen = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var rayResult = new RaycastHit();
        Physics.Raycast(new Ray(mouseToScreen, Camera.main.transform.forward), out rayResult, Mathf.Infinity);

        return rayResult.collider != null ? rayResult.collider.GetComponent<T>() : null;
    }

    public static Tile GetTileUnderMouse()
    {
        return GetComponentUnderMouse<Tile>();
    }

    public static Unit GetUnitUnderMouse()
    {
        var tile = GetTileUnderMouse();
        return tile != null && tile.Occupant != null ? tile.Occupant : null;
    }
}