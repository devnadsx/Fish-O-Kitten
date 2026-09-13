using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Bau : MonoBehaviour
{
    public GameController gameController;
    public GameObject peixeSecretoPrefab;
    public Transform[] spawnPoints;

    [Header("Referencias Visuais")]
    public IconManager iconManager;

    [Header("Configurações do Efeito Catnip")]
    public Image imagemEfeitoTela;
    public float tempoDoEfeito = 4f;
    public float velocidadeFade = 2f;
    [Range(0f, 1f)]
    public float opacidadeMaxima = 0.6f;

    [Header("Audio & Musica")]
    public AudioSource musicaPrincipal;
    public AudioSource audioSourceSFX;
    public AudioClip somAbrirBau;
    public AudioClip musicaCatnip;

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

        // 🔊 1. Toca o som de abertura do baú
        if (audioSourceSFX != null && somAbrirBau != null)
        {
            audioSourceSFX.PlayOneShot(somAbrirBau);
        }

        // 2. Spawn dos peixes secretos
        foreach (Transform ponto in spawnPoints)
        {
            if (ponto != null && gameController != null)
            {
                Instantiate(peixeSecretoPrefab, ponto.position, ponto.rotation, gameController.transform);
            }
        }

        // 🔍 3. Comunica com o GameController para revelar os peixes restantes em cena
        if (gameController != null)
        {
            gameController.RevelarPeixesRestantes();
        }

        // 4. Ativa o efeito visual e a troca de música
        if (imagemEfeitoTela != null)
        {
            StartCoroutine(EfeitoCatnipVisual());
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    IEnumerator EfeitoCatnipVisual()
    {
        // 🐱 Mudar expressão do gato para Catnip
        if (iconManager != null)
        {
            iconManager.MudarParaCatnip();
        }

        // 🎵 Pausa a música padrão e toca a música do Catnip
        if (musicaPrincipal != null && musicaPrincipal.isPlaying)
        {
            musicaPrincipal.Pause();
        }

        if (audioSourceSFX != null && musicaCatnip != null)
        {
            audioSourceSFX.clip = musicaCatnip;
            audioSourceSFX.loop = true;
            audioSourceSFX.Play();
        }

        Color cor = imagemEfeitoTela.color;
        cor.a = 0f;
        imagemEfeitoTela.color = cor;
        imagemEfeitoTela.gameObject.SetActive(true);

        // --- FADE IN ---
        while (cor.a < opacidadeMaxima)
        {
            cor.a += Time.deltaTime * velocidadeFade;
            imagemEfeitoTela.color = cor;
            yield return null;
        }

        // --- ESPERA ---
        yield return new WaitForSeconds(tempoDoEfeito);

        // --- FADE OUT ---
        while (cor.a > 0f)
        {
            cor.a -= Time.deltaTime * velocidadeFade;
            imagemEfeitoTela.color = cor;
            yield return null;
        }

        imagemEfeitoTela.gameObject.SetActive(false);

        // 🔍 Remove a aura dos peixes
        if (gameController != null)
        {
            gameController.EsconderAuraPeixes(1f / velocidadeFade);
        }

        // 🐱 Volta a expressão normal do gato
        if (iconManager != null)
        {
            iconManager.MudarParaNormal();
        }

        // 🎵 Para a música do Catnip e despausa a música principal do jogo
        if (audioSourceSFX != null)
        {
            audioSourceSFX.Stop();
            audioSourceSFX.loop = false;
        }

        if (musicaPrincipal != null)
        {
            musicaPrincipal.UnPause();
        }
    }
}