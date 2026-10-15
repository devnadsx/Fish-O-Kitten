
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [Header("Configuração do Fade")]
    public Image imagemFade;
    public float duracaoFade = 1f;

    private bool estaEmTransicao;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (imagemFade != null)
        {
            imagemFade.gameObject.SetActive(true);
            DefinirAlpha(1f);
        }
    }

    void Start()
    {
        StartCoroutine(FadeEntrada());
    }

    public void TrocarCena(string nomeCena)
    {
        if (estaEmTransicao)
            return;

        StartCoroutine(FadeSaida(nomeCena));
    }

    IEnumerator FadeEntrada()
    {
        yield return Fade(1f, 0f);
    }

    IEnumerator FadeSaida(string nomeCena)
    {
        estaEmTransicao = true;

        yield return Fade(0f, 1f);

        SceneManager.LoadScene(nomeCena);
    }

    IEnumerator Fade(float alphaInicial, float alphaFinal)
    {
        if (imagemFade == null)
            yield break;

        imagemFade.gameObject.SetActive(true);

        float tempo = 0f;

        while (tempo < duracaoFade)
        {
            tempo += Time.unscaledDeltaTime;

            float progresso = Mathf.Clamp01(tempo / duracaoFade);

            DefinirAlpha(Mathf.Lerp(
                alphaInicial,
                alphaFinal,
                progresso
            ));

            yield return null;
        }

        DefinirAlpha(alphaFinal);

        if (alphaFinal == 0f)
            imagemFade.gameObject.SetActive(false);
    }

    void DefinirAlpha(float alpha)
    {
        if (imagemFade == null)
            return;

        Color cor = imagemFade.color;
        cor.a = alpha;
        imagemFade.color = cor;
    }
}
