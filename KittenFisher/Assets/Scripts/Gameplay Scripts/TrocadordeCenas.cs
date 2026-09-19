using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocadorDeCenas : MonoBehaviour
{
    [Header("Configurações de Fade")]
    public CanvasGroup canvasGroupFade;
    public float duracaoFade = 0.5f;

    void Start()
    {
        // Revela a cena atual caso a tela comece preta
        if (canvasGroupFade != null)
        {
            canvasGroupFade.alpha = 1f;
            StartCoroutine(FadeOut());
        }
    }

    public void IrParaCena(string nomeDaCena)
    {
        // Se houver CanvasGroup atribuído, faz a transição com Fade
        if (canvasGroupFade != null)
        {
            StartCoroutine(FadeInETrocarCena(nomeDaCena));
        }
        else
        {
            // Se não tiver Fade, troca instantaneamente
            SceneManager.LoadScene(nomeDaCena);
        }
    }

    public void SairDoJogo()
    {
        Debug.Log("But kitou..");
        Application.Quit();
    }

    private IEnumerator FadeInETrocarCena(string nomeDaCena)
    {
        canvasGroupFade.blocksRaycasts = true; // Impede novos cliques durante a transição
        float tempo = 0f;

        while (tempo < duracaoFade)
        {
            tempo += Time.deltaTime;
            canvasGroupFade.alpha = Mathf.Clamp01(tempo / duracaoFade);
            yield return null;
        }

        SceneManager.LoadScene(nomeDaCena);
    }

    private IEnumerator FadeOut()
    {
        float tempo = duracaoFade;

        while (tempo > 0f)
        {
            tempo -= Time.deltaTime;
            canvasGroupFade.alpha = Mathf.Clamp01(tempo / duracaoFade);
            yield return null;
        }

        canvasGroupFade.alpha = 0f;
        canvasGroupFade.blocksRaycasts = false;
    }
}