using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockEnemy : MonoBehaviour
{
    public int reward;
    public float health = 100f;
    private Coroutine healthCoroutine;
    public bool IsAlive => health > 0;
    public List<string> items = new();

    [SerializeField] private TMP_Text _lifeEnemy;

    private void Start()
    {
        healthCoroutine = StartCoroutine(HealthUpgrade()); //Iniciando la corrutina paravq loope
        items.Add("Sword");
        items.Add("Gun");
        items.Add("Coin");
    }

    private void Update()
    {
        _lifeEnemy.text = health.ToString();
        if (IsAlive == false)
        {
            var loot = LootGenerator(Random.Range(0, 3)); // te devuelve el valor del loot q te dio
            UICanvas.Instance.SetLootUI(loot); // Busca la UICanvas y ejecuta su funcion para actualsizar el canvas
            StopCoroutine(healthCoroutine); //Se frena la corrutina cuando mure para q no lopee infinitamente
            Destroy(gameObject);
        }
    }

    //MAR
    //Generator de items 
    private IEnumerable<string> LootGenerator(int itemsLoot)
    {
        for (int i = 0; i < itemsLoot; i++)
        {
            yield return items[UnityEngine.Random.Range(0, items.Count)];
        }
    }

    //Time - Slicing
    private IEnumerator HealthUpgrade() //Corrutina para curar a los enemigos
    {
        while (true)
        {
            yield return new WaitForSeconds(4);
            health += 20;
        }
    }
}