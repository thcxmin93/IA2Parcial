using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


//MAR
public class UICanvas : MonoBehaviour
{
    [SerializeField] private WaveManager _WaveManager;
    [SerializeField] private TMP_Text _textEnemies;
    [SerializeField] private TMP_Text _itemsLoot;
    [SerializeField] private TMP_Text _enemyAnalyzed;
    public static UICanvas Instance; //Variable unica q se puede acceder a nivel global

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        _textEnemies.text = $"Enemies: {(_WaveManager.enemyList.Count)}";
    }

    public void SetLootUI(IEnumerable<string> lootColection)
    {
        foreach (var item in lootColection)
        {
            _itemsLoot.text += $"{item}\n";
        }
    }

    public void SetEnemiesAnalyzed(string enemyData)
    {
        _enemyAnalyzed.text = enemyData;
    }
}
//