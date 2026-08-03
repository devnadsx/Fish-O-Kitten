using System.Collections; // IMPORTANTE: Necessário para as Coroutines
using UnityEngine;
using UnityEngine.UI;    // IMPORTANTE: Necessário para controlar componentes de UI

public class Bau : MonoBehaviour
{
    public GameController gameController;
    public GameObject peixeSecretoPrefab;
    public Transform[] spawnPoints;

    [Header("Configurações do Efeito Catnip")]
    public Image imagemEfeitoTela;     // Arraste a imagem da UI aqui
    public float tempoDoEfeito = 4f;   // Quanto tempo a tela fica verde
    public float velocidadeFade = 2f;  // Quão rápido o efeito aparece/some
    [Range(0f, 1f)]
    public float opacidadeMaxima = 0.6f; // Intensidade do verde (0 = invisível, 1 = total)

    private bool jaAberto = false;

    void OnMouseDown()
    {
        if (!jaAberto)
        {
            AbrirBau();
        }
    }

    void AbrirBau()
    {
        jaAberto = true;
        Debug.Log("Catnip encontrado! Ativando efeitos...");

        // 1. Spawn dos peixes secretos
        foreach (Transform ponto in spawnPoints)
        {
            if (ponto != null && gameController != null)
            {
                Instantiate(peixeSecretoPrefab, ponto.position, ponto.rotation, gameController.transform);
            }
        }

        // 2. Ativa o efeito visual na tela e no gatinho
        if (imagemEfeitoTela != null)
        {
            StartCoroutine(EfeitoCatnipVisual());
        }

        GetComponent<Collider2D>().enabled = false;
    }

    // Coroutine adaptada para controlar também a expressão do gato
    IEnumerator EfeitoCatnipVisual()
    {
      

        Color cor = imagemEfeitoTela.color;
        cor.a = 0f; // Começa totalmente transparente
        imagemEfeitoTela.color = cor;
        imagemEfeitoTela.gameObject.SetActive(true);

        // --- FADE IN (Aparecendo) ---
        while (cor.a < opacidadeMaxima)
        {
            cor.a += Time.deltaTime * velocidadeFade;
            imagemEfeitoTela.color = cor;
            yield return null;
        }

        // --- ESPERA (Duração do efeito) ---
        yield return new WaitForSeconds(tempoDoEfeito);

        // --- FADE OUT (Sumindo) ---
        while (cor.a > 0f)
        {
            cor.a -= Time.deltaTime * velocidadeFade;
            imagemEfeitoTela.color = cor;
            yield return null;
        }

      

        imagemEfeitoTela.gameObject.SetActive(false); // Garante que a imagem suma do Canvas
    }
}