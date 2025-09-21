using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockEnemy : Enemy
{
    private Coroutine healthCoroutine;


    private void Start()
    {
        healthCoroutine = StartCoroutine(HealthUpgrade()); //Iniciando la corrutina paravq loope
        items.Add("Sword");
        items.Add("Gun");
        items.Add("Coin");
    }

    protected override void Update()
    {
        base.Update(); //LLama al update del objeto padre
        if (IsAlive == false)
        {
            var loot = LootGenerator(Random.Range(0, 3)); // te devuelve el valor del loot q te dio
            UICanvas.Instance.SetLootUI(loot); // Busca la UICanvas y ejecuta su funcion para actualsizar el canvas
            if (healthCoroutine != null)
            {
                StopCoroutine(healthCoroutine);
            } //Se frena la corrutina cuando mure para q no lopee infinitamente
            Destroy(gameObject);
        }
    }

    public void StopHealth()
    {
        if (healthCoroutine != null)
        {
            StopCoroutine(healthCoroutine);
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