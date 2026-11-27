
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EntrevistaPaxtonManager : MonoBehaviour
{
    [System.Serializable]
    public class ExpressaoPersonagem
    {
        public string nomeExpressao;
        public Sprite bocaFechada;
        public Sprite bocaAberta;
    }

    [System.Serializable]
    public class Personagem
    {
        public string nome;
        public List<ExpressaoPersonagem> expressoes =
            new List<ExpressaoPersonagem>();
    }

    [System.Serializable]
    public class FalaEntrevista
    {
        [Header("Fala")]
        public string nomeQuemFala;
        [TextArea(2, 5)]
        public string texto;
        public bool ehNarracao;
        public PosicaoPersonagemEntrevista posicao;
        public AudioClip somFala;

        [Header("Expressões")]
        public string expressaoMyke;
        public string expressaoPaxton;
        public string expressaoSaky;

        [Header("Movimento do Paxton")]
        public bool moverPaxton;
        public float deslocamentoX;
        public float deslocamentoY;
        public float duracaoMovimento = 0.5f;
    }

    public enum PosicaoPersonagemEntrevista
    {
        Esquerda,
        Centro,
        Direita
    }

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;

    [Header("Imagens dos Personagens")]
    public Image imagemMyke;
    public Image imagemPaxton;
    public Image imagemSaky;
    public Image imagemSakyAlternativa;

    [Header("Personagens")]
    public Personagem myke;
    public Personagem paxton;
    public Personagem saky;
    public Personagem sakyAlternativa;

    [Header("Falas da Entrevista")]
    public List<FalaEntrevista> falas = new List<FalaEntrevista>();

    [Header("Configurações de Digitação")]
    public float velocidadeEscrita = 0.03f;
    public AudioSource audioSourceFala;
    public AudioClip somFalaPadrao;

    [Header("Pulinho ao falar")]
    public float alturaPuloLetra = 8f;
    public float duracaoPuloLetra = 0.12f;

    [Header("Configurações de Movimento")]
    public float velocidadeTransicaoExpressao = 0.25f;

    [Header("Final da Entrevista")]
    public string nomeProximaCena = "";

    private int indiceFala;
    private bool escrevendo;
    private bool entrevistaFinalizada;

    private string textoCompleto;
    private Coroutine coroutineDigitacao;
    private Coroutine coroutineMovimento;

    private Coroutine coroutinePuloMyke;
    private Coroutine coroutinePuloPaxton;
    private Coroutine coroutinePuloSaky;

    [Header("Pulinho ao mudar expressão")]
    public float alturaPuloExpressao = 18f;
    public float duracaoPuloExpressao = 0.22f;

    private Vector2 posicaoOriginalMyke;
    private Vector2 posicaoOriginalPaxton;
    private Vector2 posicaoOriginalSaky;
    private Vector2 posicaoOriginalSakyAlternativa;

    private Vector2 posicaoBaseMyke;
    private Vector2 posicaoBasePaxton;
    private Vector2 posicaoBaseSaky;
    private Vector2 posicaoBaseSakyAlternativa;

    private float puloLetraMyke;
    private float puloLetraPaxton;
    private float puloLetraSaky;

    private float puloExpressaoMyke;
    private float puloExpressaoPaxton;
    private float puloExpressaoSaky;

    private Coroutine coroutineExpressaoMyke;
    private Coroutine coroutineExpressaoPaxton;
    private Coroutine coroutineExpressaoSaky;

    private ExpressaoPersonagem expressaoAtualMyke;
    private ExpressaoPersonagem expressaoAtualPaxton;
    private ExpressaoPersonagem expressaoAtualSaky;

    private bool sakyUsaImagemAlternativa;

    private void Awake()
    {
        if (imagemMyke != null)
            posicaoOriginalMyke = imagemMyke.rectTransform.anchoredPosition;

        if (imagemPaxton != null)
            posicaoOriginalPaxton = imagemPaxton.rectTransform.anchoredPosition;

        if (imagemSaky != null)
            posicaoOriginalSaky = imagemSaky.rectTransform.anchoredPosition;

        if (imagemSakyAlternativa != null)
            posicaoOriginalSakyAlternativa =
                imagemSakyAlternativa.rectTransform.anchoredPosition;

        posicaoBaseMyke = posicaoOriginalMyke;
        posicaoBasePaxton = posicaoOriginalPaxton;
        posicaoBaseSaky = posicaoOriginalSaky;
        posicaoBaseSakyAlternativa = posicaoOriginalSakyAlternativa;

        sakyUsaImagemAlternativa = false;

        if (imagemSaky != null)
            imagemSaky.gameObject.SetActive(true);

        if (imagemSakyAlternativa != null)
            imagemSakyAlternativa.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        AplicarPosicaoComPulinho(imagemMyke, posicaoBaseMyke,
            puloLetraMyke + puloExpressaoMyke);

        AplicarPosicaoComPulinho(imagemPaxton, posicaoBasePaxton,
            puloLetraPaxton + puloExpressaoPaxton);

        if (sakyUsaImagemAlternativa)
        {
            AplicarPosicaoComPulinho(imagemSakyAlternativa,
                posicaoBaseSakyAlternativa,
                puloLetraSaky + puloExpressaoSaky);
        }
        else
        {
            AplicarPosicaoComPulinho(imagemSaky, posicaoBaseSaky,
                puloLetraSaky + puloExpressaoSaky);
        }
    }

    private void AplicarPosicaoComPulinho(Image imagem, Vector2 posicaoBase, float altura)
    {
        if (imagem == null)
            return;

        imagem.rectTransform.anchoredPosition =
            posicaoBase + Vector2.up * altura;
    }

    private void Start()
    {
        if (falas.Count > 0)
        {
            indiceFala = 0;
            entrevistaFinalizada = false;

            if (painelBalaoFala != null)
                painelBalaoFala.SetActive(true);

            MostrarFala();
        }
        else
        {
            FinalizarEntrevista();
        }
    }

    private void Update()
    {
        if (entrevistaFinalizada)
            return;

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            AvancarFala();
        }
    }

    public void AvancarFala()
    {
        if (entrevistaFinalizada)
            return;

        if (escrevendo)
        {
            CompletarTexto();
            return;
        }

        indiceFala++;

        if (indiceFala < falas.Count)
        {
            MostrarFala();
        }
        else
        {
            FinalizarEntrevista();
        }
    }

    private void MostrarFala()
    {
        FalaEntrevista fala = falas[indiceFala];

        textoCompleto = fala.texto;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(true);

        if (textoNomePersonagem != null)
        {
            textoNomePersonagem.text =
                fala.ehNarracao ? "" : fala.nomeQuemFala;
        }

        AplicarExpressoes(fala);
        AplicarMovimento(fala);

        if (coroutineDigitacao != null)
            StopCoroutine(coroutineDigitacao);

        coroutineDigitacao = StartCoroutine(DigitarTexto(fala));
    }

    private IEnumerator DigitarTexto(FalaEntrevista fala)
    {
        escrevendo = true;

        if (textoBalao != null)
            textoBalao.text = "";

        Image imagemAtiva = ObterImagem(fala.nomeQuemFala);

        foreach (char letra in fala.texto)
        {
            if (textoBalao != null)
                textoBalao.text += letra;

            if (!fala.ehNarracao && char.IsLetterOrDigit(letra))
            {
                ExpressaoPersonagem expressao = ObterExpressaoAtual(fala.nomeQuemFala);

                if (imagemAtiva != null && expressao != null &&
                    expressao.bocaAberta != null)
                {
                    imagemAtiva.sprite = expressao.bocaAberta;
                }

                AudioClip som = fala.somFala != null
                    ? fala.somFala
                    : somFalaPadrao;

                if (audioSourceFala != null && som != null)
                    audioSourceFala.PlayOneShot(som);

                IniciarPulinho(fala.nomeQuemFala);
            }
            else
            {
                RestaurarExpressaoDoFalante(fala.nomeQuemFala);
            }

            yield return new WaitForSeconds(velocidadeEscrita);

            RestaurarExpressaoDoFalante(fala.nomeQuemFala);
        }

        RestaurarExpressaoDoFalante(fala.nomeQuemFala);

        escrevendo = false;
    }

    private void CompletarTexto()
    {
        if (coroutineDigitacao != null)
            StopCoroutine(coroutineDigitacao);

        if (textoBalao != null)
            textoBalao.text = textoCompleto;

        RestaurarExpressaoDoFalante(falas[indiceFala].nomeQuemFala);

        escrevendo = false;
    }

    private void IniciarPulinho(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return;

        RectTransform personagem = null;
        Coroutine puloAtual = null;

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
        {
            if (imagemMyke == null) return;
            personagem = imagemMyke.rectTransform;
            puloAtual = coroutinePuloMyke;
        }
        else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
        {
            if (imagemPaxton == null) return;
            personagem = imagemPaxton.rectTransform;
            puloAtual = coroutinePuloPaxton;
        }
        else if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
        {
            Image imagem = ObterImagem("Saky");
            if (imagem == null) return;

            personagem = imagem.rectTransform;
            puloAtual = coroutinePuloSaky;
        }
        else return;

        if (puloAtual != null)
            StopCoroutine(puloAtual);

        Coroutine novoPulo = StartCoroutine(Pulinho(personagem));

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            coroutinePuloMyke = novoPulo;
        else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            coroutinePuloPaxton = novoPulo;
        else
            coroutinePuloSaky = novoPulo;
    }

    private IEnumerator Pulinho(RectTransform personagem)
    {
        if (personagem == null)
            yield break;

        string nome = ObterNomePeloRect(personagem);
        yield return AnimarPulinho(nome, alturaPuloLetra, duracaoPuloLetra, false);
    }

    private string ObterNomePeloRect(RectTransform rect)
    {
        if (imagemMyke != null && rect == imagemMyke.rectTransform) return "Myke";
        if (imagemPaxton != null && rect == imagemPaxton.rectTransform) return "Paxton";
        if (imagemSaky != null && rect == imagemSaky.rectTransform) return "Saky";
        if (imagemSakyAlternativa != null &&
            rect == imagemSakyAlternativa.rectTransform) return "Saky";

        return "";
    }

    private IEnumerator AnimarPulinho(string nome, float altura, float duracao, bool aoMudarExpressao)
    {
        float tempo = 0f;
        duracao = Mathf.Max(0.01f, duracao);

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracao);
            float valor = Mathf.Sin(progresso * Mathf.PI) * altura;
            DefinirAlturaPulinho(nome, valor, aoMudarExpressao);
            yield return null;
        }

        DefinirAlturaPulinho(nome, 0f, aoMudarExpressao);
    }

    private void DefinirAlturaPulinho(string nome, float valor, bool aoMudarExpressao)
    {
        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
        {
            if (aoMudarExpressao) puloExpressaoMyke = valor;
            else puloLetraMyke = valor;
        }
        else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
        {
            if (aoMudarExpressao) puloExpressaoPaxton = valor;
            else puloLetraPaxton = valor;
        }
        else if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
        {
            if (aoMudarExpressao) puloExpressaoSaky = valor;
            else puloLetraSaky = valor;
        }
    }

    private void IniciarPulinhoExpressao(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return;

        Coroutine atual = null;

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            atual = coroutineExpressaoMyke;
        else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            atual = coroutineExpressaoPaxton;
        else if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
            atual = coroutineExpressaoSaky;

        if (atual != null)
            StopCoroutine(atual);

        Coroutine nova = StartCoroutine(
            AnimarPulinho(nome, alturaPuloExpressao, duracaoPuloExpressao, true)
        );

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            coroutineExpressaoMyke = nova;
        else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            coroutineExpressaoPaxton = nova;
        else if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
            coroutineExpressaoSaky = nova;
    }

    private void AplicarExpressoes(FalaEntrevista fala)
    {
        if (!string.IsNullOrWhiteSpace(fala.expressaoMyke))
        {
            ExpressaoPersonagem nova = EncontrarExpressao(myke, fala.expressaoMyke);
            if (nova != null)
                DefinirExpressao("Myke", nova);
        }

        if (!string.IsNullOrWhiteSpace(fala.expressaoSaky))
        {
            bool alternativa;
            ExpressaoPersonagem nova =
                EncontrarExpressaoSaky(fala.expressaoSaky, out alternativa);

            if (nova != null)
                DefinirExpressao("Saky", nova);
        }
        else if (expressaoAtualSaky == null)
        {
            if (saky != null && saky.expressoes != null &&
                saky.expressoes.Count > 0)
            {
                DefinirExpressao("Saky", saky.expressoes[0]);
            }
        }

        if (!string.IsNullOrWhiteSpace(fala.expressaoPaxton))
        {
            ExpressaoPersonagem nova = EncontrarExpressao(paxton, fala.expressaoPaxton);
            if (nova != null)
                DefinirExpressao("Paxton", nova);
        }

        RestaurarExpressaoDoFalante(fala.nomeQuemFala);
    }

    private void DefinirExpressao(string nome, ExpressaoPersonagem nova)
    {
        if (nova == null)
            return;

        ExpressaoPersonagem anterior = ObterExpressaoAtual(nome);

        if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
        {
            bool alternativa;
            EncontrarExpressaoSaky(nova.nomeExpressao, out alternativa);

            sakyUsaImagemAlternativa = alternativa;

            if (imagemSaky != null)
                imagemSaky.gameObject.SetActive(!alternativa);

            if (imagemSakyAlternativa != null)
                imagemSakyAlternativa.gameObject.SetActive(alternativa);

            expressaoAtualSaky = nova;
        }
        else if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
        {
            if (anterior == nova)
                return;

            expressaoAtualMyke = nova;
        }
        else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
        {
            if (anterior == nova)
                return;

            expressaoAtualPaxton = nova;
        }

        if (anterior == nova)
        {
            RestaurarExpressao(nome);
            return;
        }

        RestaurarExpressao(nome);
        IniciarPulinhoExpressao(nome);
    }

    private ExpressaoPersonagem EncontrarExpressaoSaky(
        string nomeExpressao, out bool alternativa)
    {
        alternativa = false;

        ExpressaoPersonagem expressao = EncontrarExpressao(saky, nomeExpressao);

        if (expressao != null)
            return expressao;

        expressao = EncontrarExpressaoSemAviso(sakyAlternativa, nomeExpressao);

        if (expressao != null)
        {
            alternativa = true;
            return expressao;
        }

        Debug.LogWarning("Expressão '" + nomeExpressao +
            "' não encontrada nas listas da Saky.");

        return null;
    }

    private ExpressaoPersonagem EncontrarExpressaoSemAviso(
        Personagem personagem, string nomeExpressao)
    {
        if (personagem == null || personagem.expressoes == null ||
            string.IsNullOrWhiteSpace(nomeExpressao))
            return null;

        foreach (ExpressaoPersonagem expressao in personagem.expressoes)
        {
            if (expressao != null &&
                nomeExpressao.Equals(expressao.nomeExpressao,
                    System.StringComparison.OrdinalIgnoreCase))
                return expressao;
        }

        return null;
    }

    private ExpressaoPersonagem EncontrarExpressao(
        Personagem personagem, string nomeExpressao)
    {
        if (personagem == null || personagem.expressoes == null ||
            string.IsNullOrWhiteSpace(nomeExpressao))
            return null;

        foreach (ExpressaoPersonagem expressao in personagem.expressoes)
        {
            if (expressao != null &&
                nomeExpressao.Equals(expressao.nomeExpressao,
                    System.StringComparison.OrdinalIgnoreCase))
                return expressao;
        }

        Debug.LogWarning("Expressão '" + nomeExpressao +
            "' não encontrada para " + personagem.nome + ".");

        return null;
    }

    private ExpressaoPersonagem ObterExpressaoAtual(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return null;

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            return expressaoAtualMyke;

        if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            return expressaoAtualPaxton;

        if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
            return expressaoAtualSaky;

        return null;
    }

    private void RestaurarExpressao(string nome)
    {
        Image imagem = ObterImagem(nome);
        ExpressaoPersonagem expressao = ObterExpressaoAtual(nome);

        if (imagem != null && expressao != null && expressao.bocaFechada != null)
            imagem.sprite = expressao.bocaFechada;
    }

    private void RestaurarExpressaoDoFalante(string nome)
    {
        RestaurarExpressao(nome);
    }

    private void AplicarMovimento(FalaEntrevista fala)
    {
        if (!fala.moverPaxton || imagemPaxton == null)
            return;

        if (coroutineMovimento != null)
            StopCoroutine(coroutineMovimento);

        Vector2 destino = posicaoOriginalPaxton +
            new Vector2(fala.deslocamentoX, fala.deslocamentoY);

        coroutineMovimento = StartCoroutine(
            MoverPaxton(destino, fala.duracaoMovimento)
        );
    }

    private IEnumerator MoverPaxton(Vector2 destino, float duracao)
    {
        Vector2 inicio = posicaoBasePaxton;

        if (duracao <= 0f)
        {
            posicaoBasePaxton = destino;
            yield break;
        }

        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            float progresso = Mathf.Clamp01(tempo / duracao);

            posicaoBasePaxton = Vector2.Lerp(
                inicio,
                destino,
                progresso
            );

            yield return null;
        }

        posicaoBasePaxton = destino;
    }

    private Personagem ObterPersonagem(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return null;

        if (myke != null &&
            nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            return myke;

        if (paxton != null &&
            nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            return paxton;

        if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
            return sakyUsaImagemAlternativa ? sakyAlternativa : saky;

        return null;
    }

    private Image ObterImagem(string nome)
    {
        if (string.IsNullOrEmpty(nome))
            return null;

        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
            return imagemMyke;

        if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
            return imagemPaxton;

        if (nome.Equals("Saky", System.StringComparison.OrdinalIgnoreCase))
            return sakyUsaImagemAlternativa
                ? imagemSakyAlternativa
                : imagemSaky;

        return null;
    }

    private void FinalizarEntrevista()
    {
        entrevistaFinalizada = true;
        escrevendo = false;

        if (painelBalaoFala != null)
            painelBalaoFala.SetActive(false);

        if (!string.IsNullOrEmpty(nomeProximaCena))
            SceneManager.LoadScene(nomeProximaCena);
    }

    public void ReiniciarEntrevista()
    {
        if (coroutineDigitacao != null)
            StopCoroutine(coroutineDigitacao);

        if (coroutineMovimento != null)
            StopCoroutine(coroutineMovimento);

        if (coroutinePuloMyke != null) StopCoroutine(coroutinePuloMyke);
        if (coroutinePuloPaxton != null) StopCoroutine(coroutinePuloPaxton);
        if (coroutinePuloSaky != null) StopCoroutine(coroutinePuloSaky);
        if (coroutineExpressaoMyke != null) StopCoroutine(coroutineExpressaoMyke);
        if (coroutineExpressaoPaxton != null) StopCoroutine(coroutineExpressaoPaxton);
        if (coroutineExpressaoSaky != null) StopCoroutine(coroutineExpressaoSaky);

        posicaoBaseMyke = posicaoOriginalMyke;
        posicaoBasePaxton = posicaoOriginalPaxton;
        posicaoBaseSaky = posicaoOriginalSaky;
        posicaoBaseSakyAlternativa = posicaoOriginalSakyAlternativa;

        puloLetraMyke = puloLetraPaxton = puloLetraSaky = 0f;
        puloExpressaoMyke = puloExpressaoPaxton = puloExpressaoSaky = 0f;

        sakyUsaImagemAlternativa = false;

        if (imagemSaky != null)
            imagemSaky.gameObject.SetActive(true);

        if (imagemSakyAlternativa != null)
            imagemSakyAlternativa.gameObject.SetActive(false);

        expressaoAtualMyke = myke != null && myke.expressoes != null &&
            myke.expressoes.Count > 0 ? myke.expressoes[0] : null;

        expressaoAtualPaxton = paxton != null && paxton.expressoes != null &&
            paxton.expressoes.Count > 0 ? paxton.expressoes[0] : null;

        expressaoAtualSaky = saky != null && saky.expressoes != null &&
            saky.expressoes.Count > 0 ? saky.expressoes[0] : null;

        RestaurarExpressao("Myke");
        RestaurarExpressao("Paxton");
        RestaurarExpressao("Saky");

        indiceFala = 0;
        entrevistaFinalizada = false;

        MostrarFala();
    }
}
