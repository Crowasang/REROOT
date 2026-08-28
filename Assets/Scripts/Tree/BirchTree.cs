using UnityEngine;

public class BirchTree : Tree
{
    
    protected override void OnDestroyed()
    {
        Destroy(gameObject);
    }
}
