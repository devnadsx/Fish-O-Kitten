using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsconderijosManager : MonoBehaviour
{
    public static EsconderijosManager Instance;

    [Header("Configuração Global")]
    public float tempoReveladoPadrao = 3f; // Tempo que qualquer item fica sumido

    [Header("Lista de Itens do Cenário")]
    public List<ItemCenario> listaDeItens = new List<ItemCenario>();

    [HideInInspector]
    public bool interacaoBloqueada = false; // Quando true (ex: durante diálogos), impede de vasculhar!

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Busca automaticamente todos os ItemCenario do jogo se você não quiser arrastar um por um!
    [ContextMenu("Buscar Todos Itens da Cena")]
    public void BuscarTodosOsItens()
    {
        listaDeItens.Clear();
        listaDeItens.AddRange(FindObjectsByType<ItemCenario>(FindObjectsSortMode.None));
        Debug.Log($"Encontrados {listaDeItens.Count} itens de cenário na cena!");
    }

    // Bloqueia ou libera o clique em todos os esconderijos de uma vez
    public void DefinirInteracaoLiberada(bool liberado)
    {
        interacaoBloqueada = !liberado;
    }
}