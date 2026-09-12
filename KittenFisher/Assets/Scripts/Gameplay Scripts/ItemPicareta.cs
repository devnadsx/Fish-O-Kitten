using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPicareta : MonoBehaviour
{
    [Header("Efeitos Sonoros")]
    public AudioSource audioSource;
    public AudioClip somColeta;

    void OnMouseDown()
    {
        if (EsconderijosManager.Instance != null && EsconderijosManager.Instance.interacaoBloqueada) return;

        ColetarPicareta();
    }

    void ColetarPicareta()
    {
        // 1. Marca no inventário que possui a picareta
        if (GerenciadorInventario.Instance != null)
        {
            GerenciadorInventario.Instance.temPicareta = true;
        }

        // 2. Toca o som de coleta
        if (somColeta != null)
        {
            AudioSource.PlayClipAtPoint(somColeta, transform.position);
        }

        // 3. Inicia a fala do gatinho informando que achou a ferramenta
        if (DialogoManager.Instance != null)
        {
            List<LineDialogo> falaPicareta = new List<LineDialogo>()
            {
                new LineDialogo
                {
                    nomeQuemFala = "Myke",
                    texto = "Great! I found a pickaxe! Now I can break those big rocks in my way!",
                    ehNarracao = false,
                    posicao = PosicaoPersonagem.Centro
                }
            };

            DialogoManager.Instance.IniciarSequenciaDialogo(falaPicareta);
        }

        // 4. Desativa o item da tela
        gameObject.SetActive(false);
    }
}