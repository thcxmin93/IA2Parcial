using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    public float health;
    public int reward;
    public List<string> items = new();
    [SerializeField] private TMP_Text _lifeEnemy;
    public bool IsAlive => health > 0;


    protected virtual void Update() //Es para q las clases hijas puedan heredar
    {
        _lifeEnemy.text = health.ToString();
    }

    //MAR
    //Generator de items 
    protected IEnumerable<string> LootGenerator(int itemsLoot) //Protected para q los hijos puedan usalreo
    {
        for (int i = 0; i < itemsLoot; i++)
        {
            yield return items[UnityEngine.Random.Range(0, items.Count)];
        }
    }
    //
}