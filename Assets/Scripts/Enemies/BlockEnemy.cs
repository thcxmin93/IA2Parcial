using System;
using UnityEngine;

public class BlockEnemy : MonoBehaviour
{
    public int reward;
    public float health = 100f;
    public bool IsAlive => health > 0;

    private void Update()
    {
        if (IsAlive == false)
        {
            Destroy(gameObject);
        }
    }
}
