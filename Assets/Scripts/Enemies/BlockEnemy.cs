using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockEnemy : MonoBehaviour
{
    public int reward;
    public float health = 100f;
    public bool IsAlive => health > 0;
    public List<string> items = new();

    private void Start()
    {
        items.Add("Sword");
        items.Add("Gun");
        items.Add("Coin");
    }

    private void Update()
    {
        if (IsAlive == false)
        {
            var loot = LootGenerator(Random.Range(0,3)); // te devuelve el valor del loot q te dio
            UICanvas.Instance.SetLootUI(loot); // Busca la UICanvas y ejecuta su funcion para actualsizar el canvas
            Destroy(gameObject);
        }
    }
    
    //Generator de items
    private IEnumerable<string> LootGenerator(int itemsLoot)
    {
        for (int i = 0; i < itemsLoot; i++)
        {
            yield return items[UnityEngine.Random.Range(0, items.Count)];
        }
    }
}