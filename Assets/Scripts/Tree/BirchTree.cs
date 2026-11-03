using UnityEngine;

public class BirchTree : Trees
{
    
    protected override void OnDestroyed()
    {
        Destroy(gameObject);
    }
}
