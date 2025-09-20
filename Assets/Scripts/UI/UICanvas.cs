using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICanvas : MonoBehaviour
{
    [SerializeField] private WaveManager _WaveManager;
    [SerializeField] private TMP_Text _textEnemies;
    
    private void Update()
    {
        _textEnemies.text = $"Enemies: {(_WaveManager.enemyList.Count)}";
    }
}