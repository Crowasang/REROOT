using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeSpawner : MonoBehaviour
{
    [SerializeField] private BirchTree _birchTree;

    [SerializeField] private BoxCollider2D _spawnArea;

    [SerializeField] private int _treeCount = 10;

    private void Start()
    {
        TreeSapwn();
    }

    private void TreeSapwn()
    {
        for (int i = 0; i < _treeCount; i++)
        {
            Vector2 spawnPosition = GetRandomPoint();
            
            Instantiate(_birchTree, spawnPosition, Quaternion.identity);
        }
    }
    
    private Vector2 GetRandomPoint()
    {
        Bounds _bounds = _spawnArea.bounds;

        return new Vector2(
            Random.Range(_bounds.min.x, _bounds.max.x),
            Random.Range(_bounds.min.y, _bounds.max.y));
    }
}
