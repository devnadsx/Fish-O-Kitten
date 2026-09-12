using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GerenciadorTrocaCenario : MonoBehaviour
{
    [Header("Configurações do Fade")]
    public CanvasGroup painelFade;
    public float duracaoFade = 0.5f;

    [Header("Cenários")]
    public GameObject cenarioExterior;
    public GameObject cenarioInteriorNavio;

    [Header("Efeitos Visuais")]
    public GameObject filtroAmbienteNavio; // Imagem semitransparente azul/escura

    private bool transicionando = false;

    // Método chamado pelo clique na porta/entrada do navio
    public void EntrarNoNavio()
    {
        if (!transicionando)
        {
            StartCoroutine(RotinaTransicao(true));
        }
    }

    // Método chamado pelo clique na saída do navio
    public void SairDoNavio()
    {
        if (!transicionando)
        {
            StartCoroutine(RotinaTransicao(false));
        }
    }

    IEnumerator RotinaTransicao(bool entrandoNoNavio)
    {
        transicionando = true;

        // 1. FADE OUT (Tela fica preta)
        float tempo = 0;
        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;
            painelFade.alpha = Mathf.Lerp(0f, 1f, tempo / duracaoFade);
            yield return null;
        }
        painelFade.alpha = 1f;

        // 2. TROCA DE CENÁRIO (Acontece no escuro)
        if (entrandoNoNavio)
        {
            if (cenarioExterior != null) cenarioExterior.SetActive(false);
            if (cenarioInteriorNavio != null) cenarioInteriorNavio.SetActive(true);
            if (filtroAmbienteNavio != null) filtroAmbienteNavio.SetActive(true);
        }
        else
        {
            if (cenarioInteriorNavio != null) cenarioInteriorNavio.SetActive(false);
            if (cenarioExterior != null) cenarioExterior.SetActive(true);
            if (filtroAmbienteNavio != null) filtroAmbienteNavio.SetActive(false);
        }

        yield return new WaitForSeconds(0.1f); // Pausa curtinha para estabilizar

        // 3. FADE IN (Tela volta ao normal)
        tempo = 0;
        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;
            painelFade.alpha = Mathf.Lerp(1f, 0f, tempo / duracaoFade);
            yield return null;
        }
        painelFade.alpha = 0f;

        transicionando = false;
    }
}