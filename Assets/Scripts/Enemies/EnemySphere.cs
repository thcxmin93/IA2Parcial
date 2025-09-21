using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySphere : Enemy
{
    protected override void Update()//Heredan del padre(override)
    {
        base.Update();//LLama al update del objeto padre
        if (IsAlive == false)
        {
            Destroy(gameObject);
        }
    }
}
