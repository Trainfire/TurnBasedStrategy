using UnityEngine;
using System;

public class GameboardData : ScriptableObject
{
    public int GridSize { get { return _gridSize; } }
    [SerializeField] private int _gridSize;

    public int MaxTurns { get { return _maxTurns; } }
    [SerializeField] private int _maxTurns;

    public GameboardDataPrefabs Prefabs { get { return _prefabs; } }
    [SerializeField] private GameboardDataPrefabs _prefabs;
}

[Serializable]
public class GameboardDataPrefabs
{
    public MechData DefaultMech { get { return _defaultMech; } }
    public Building DefaultBuilding { get { return _defaultBuilding; } }
    public EnemyData DefaultEnemy { get { return _defaultEnemy; } }
    public Tile Tile { get { return _tile; } }

    [SerializeField] private MechData _defaultMech;
    [SerializeField] private Building _defaultBuilding;
    [SerializeField] private EnemyData _defaultEnemy;
    [SerializeField] private Tile _tile;
}