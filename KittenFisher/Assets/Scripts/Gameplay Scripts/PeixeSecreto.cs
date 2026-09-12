using System.Collections;
using UnityEngine;

public class PeixeSecreto : MonoBehaviour
{
    public GameObject auraEfeito; // Arraste o objeto da Aura (filho do peixe) aqui no Inspector
    public float tempoDaAura = 4f; // Quantos segundos a aura fica visível

    void Start()
    {
        // 1. Acha o GameController e avisa que um novo peixe entrou no jogo
        GameController gameController = GetComponentInParent<GameController>();
        if (gameController != null)
        {
            gameController.FishNumber++; // Aumenta o total necessário para vencer!
        }

        // 2. Inicia o cronômetro para apagar a aura
        if (auraEfeito != null)
        {
            StartCoroutine(ContadorAura());
        }
    }

    IEnumerator ContadorAura()
    {
        auraEfeito.SetActive(true); // Garante que a aura apareceu
        yield return new WaitForSeconds(tempoDaAura); // Espera os segundos
        auraEfeito.SetActive(false); // Desliga apenas a aura, o peixe continua lá!
    }
}