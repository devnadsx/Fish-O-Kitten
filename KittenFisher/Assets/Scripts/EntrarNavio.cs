using UnityEngine;

public class EntrarNavio : MonoBehaviour
{
    public Transform pontoEntradaInterior; // Ponto vazio dentro do navio
    public GameObject jogador;

    public void EntrarNoNavio()
    {
        // Teleporta o jogador instantaneamente para dentro do navio
        jogador.transform.position = pontoEntradaInterior.position;
    }
}