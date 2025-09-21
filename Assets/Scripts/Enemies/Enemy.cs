using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health;

    public List<string> items = new();

    //MAR
    //Generator de items 
    protected IEnumerable<string> LootGenerator(int itemsLoot) //protected para q los hijos puedan usalreo
    {
        for (int i = 0; i < itemsLoot; i++)
        {
            yield return items[UnityEngine.Random.Range(0, items.Count)];
        }
    }
}