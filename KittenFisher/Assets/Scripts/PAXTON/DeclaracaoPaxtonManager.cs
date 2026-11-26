using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DeclaracaoPaxtonManager : MonoBehaviour
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
        public List<ExpressaoPersonagem> expressoes = new List<ExpressaoPersonagem>();
    }
    public void TestarClique()
    {
        Debug.Log("O BOTÃO ESTÁ FUNCIONANDO!");
    }

    [System.Serializable]
    public class FalaDeclaracao
    {
        [Header("Fala")]
        public string nomeQuemFala;
        [TextArea(2, 5)] public string texto;
        public bool ehNarracao;
        public AudioClip somFala;

        [Header("Expressões")]
        public string expressaoMyke;
        public string expressaoPaxton;

        [Header("Movimento do Myke")]
        public bool moverMyke;
        public float deslocamentoXMyke;
        public float deslocamentoYMyke;
        public float duracaoMovimentoMyke = 0.5f;

        [Header("Movimento do Paxton")]
        public bool moverPaxton;
        public float deslocamentoXPaxton;
        public float deslocamentoYPaxton;
        public float duracaoMovimentoPaxton = 0.5f;
    }

    [Header("UI do Diálogo")]
    public GameObject painelBalaoFala;
    public TextMeshProUGUI textoNomePersonagem;
    public TextMeshProUGUI textoBalao;

    [Header("Imagens dos Personagens")]
    public Image imagemMyke;
    public Image imagemPaxton;

    [Header("Personagens e expressões")]
    public Personagem myke;
    public Personagem paxton;

    [Header("Falas antes da escolha")]
    public List<FalaDeclaracao> falasAntesDaEscolha = new List<FalaDeclaracao>();

    [Header("Falas caso aceite")]
    public List<FalaDeclaracao> falasSeAceitar = new List<FalaDeclaracao>();

    [Header("Falas caso recuse")]
    public List<FalaDeclaracao> falasSeRecusar = new List<FalaDeclaracao>();

    [Header("Escolha do jogador")]
    public GameObject painelEscolha;
    public Button botaoAceitar;
    public Button botaoRecusar;

    [Header("Configurações de digitação")]
    public float velocidadeEscrita = 0.03f;
    public AudioSource audioSourceFala;
    public AudioClip somFalaPadrao;

    [Header("Pulinho ao falar")]
    public float alturaPuloLetra = 8f;
    public float duracaoPuloLetra = 0.12f;

    [Header("Pulinho ao mudar expressão")]
    public float alturaPuloExpressao = 18f;
    public float duracaoPuloExpressao = 0.22f;

    [Header("Final")]
    public string nomeProximaCena = "";

    private enum Etapa { AntesDaEscolha, Aceitou, Recusou, Finalizado }
    private Etapa etapaAtual;
    private List<FalaDeclaracao> falasAtuais;
    private int indiceFala;
    private bool escrevendo;
    private bool aguardandoEscolha;
    private bool finalizado;
    private string textoCompleto;
    private Coroutine coroutineDigitacao;

    private Vector2 posicaoOriginalMyke, posicaoOriginalPaxton;
    private Vector2 posicaoBaseMyke, posicaoBasePaxton;
    private float puloLetraMyke, puloLetraPaxton;
    private float puloExpressaoMyke, puloExpressaoPaxton;

    private Coroutine coroutinePuloLetraMyke, coroutinePuloLetraPaxton;
    private Coroutine coroutinePuloExpressaoMyke, coroutinePuloExpressaoPaxton;
    private Coroutine coroutineMovimentoMyke, coroutineMovimentoPaxton;

    private ExpressaoPersonagem expressaoAtualMyke, expressaoAtualPaxton;

    private void Awake()
    {
        if (imagemMyke != null) posicaoOriginalMyke = imagemMyke.rectTransform.anchoredPosition;
        if (imagemPaxton != null) posicaoOriginalPaxton = imagemPaxton.rectTransform.anchoredPosition;
        posicaoBaseMyke = posicaoOriginalMyke;
        posicaoBasePaxton = posicaoOriginalPaxton;

        if (botaoAceitar != null) botaoAceitar.onClick.AddListener(AceitarDeclaracao);
        if (botaoRecusar != null) botaoRecusar.onClick.AddListener(RecusarDeclaracao);
    }

    private void Start()
    {
        expressaoAtualMyke = PrimeiraExpressao(myke);
        expressaoAtualPaxton = PrimeiraExpressao(paxton);
        RestaurarExpressao("Myke");
        RestaurarExpressao("Paxton");

        etapaAtual = Etapa.AntesDaEscolha;
        falasAtuais = falasAntesDaEscolha;
        indiceFala = 0;

        if (painelEscolha != null) painelEscolha.SetActive(false);
        if (falasAtuais != null && falasAtuais.Count > 0) MostrarFala();
        else MostrarEscolha();
    }

    private void Update()
    {
        if (finalizado || aguardandoEscolha) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) AvancarFala();
    }

    private void LateUpdate()
    {
        AplicarPosicaoComPulinho(imagemMyke, posicaoBaseMyke, puloLetraMyke + puloExpressaoMyke);
        AplicarPosicaoComPulinho(imagemPaxton, posicaoBasePaxton, puloLetraPaxton + puloExpressaoPaxton);
    }

    private void AplicarPosicaoComPulinho(Image imagem, Vector2 basePosicao, float altura)
    {
        if (imagem != null) imagem.rectTransform.anchoredPosition = basePosicao + Vector2.up * altura;
    }

    public void AvancarFala()
    {
        if (finalizado || aguardandoEscolha) return;
        if (escrevendo) { CompletarTexto(); return; }

        indiceFala++;
        if (falasAtuais != null && indiceFala < falasAtuais.Count) MostrarFala();
        else if (etapaAtual == Etapa.AntesDaEscolha) MostrarEscolha();
        else FinalizarDialogo();
    }

    private void MostrarFala()
    {
        if (falasAtuais == null || indiceFala < 0 || indiceFala >= falasAtuais.Count) return;
        FalaDeclaracao fala = falasAtuais[indiceFala];
        textoCompleto = fala.texto;

        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (textoNomePersonagem != null) textoNomePersonagem.text = fala.ehNarracao ? "" : fala.nomeQuemFala;

        AplicarExpressoes(fala);
        AplicarMovimentos(fala);

        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        coroutineDigitacao = StartCoroutine(DigitarTexto(fala));
    }

    private IEnumerator DigitarTexto(FalaDeclaracao fala)
    {
        escrevendo = true;
        if (textoBalao != null) textoBalao.text = "";
        Image imagemAtiva = ObterImagem(fala.nomeQuemFala);

        foreach (char letra in fala.texto)
        {
            if (textoBalao != null) textoBalao.text += letra;

            if (!fala.ehNarracao && char.IsLetterOrDigit(letra))
            {
                ExpressaoPersonagem expressao = ObterExpressaoAtual(fala.nomeQuemFala);
                if (imagemAtiva != null && expressao != null && expressao.bocaAberta != null)
                    imagemAtiva.sprite = expressao.bocaAberta;

                AudioClip som = fala.somFala != null ? fala.somFala : somFalaPadrao;
                if (audioSourceFala != null && som != null) audioSourceFala.PlayOneShot(som);
                IniciarPulinhoLetra(fala.nomeQuemFala);
            }
            else RestaurarExpressao(fala.nomeQuemFala);

            yield return new WaitForSeconds(velocidadeEscrita);
            RestaurarExpressao(fala.nomeQuemFala);
        }

        RestaurarExpressao(fala.nomeQuemFala);
        escrevendo = false;
    }

    private void CompletarTexto()
    {
        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        if (textoBalao != null) textoBalao.text = textoCompleto;
        if (falasAtuais != null && indiceFala >= 0 && indiceFala < falasAtuais.Count)
            RestaurarExpressao(falasAtuais[indiceFala].nomeQuemFala);
        escrevendo = false;
    }

    private void MostrarEscolha()
    {
        escrevendo = false;
        aguardandoEscolha = true;
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        if (painelEscolha != null) painelEscolha.SetActive(true);
    }

    public void AceitarDeclaracao()
    {
        if (!aguardandoEscolha || finalizado) return;
        IniciarRotaDeEscolha(Etapa.Aceitou, falasSeAceitar);
    }

    public void RecusarDeclaracao()
    {
        if (!aguardandoEscolha || finalizado) return;
        IniciarRotaDeEscolha(Etapa.Recusou, falasSeRecusar);
    }

    private void IniciarRotaDeEscolha(Etapa novaEtapa, List<FalaDeclaracao> novasFalas)
    {
        aguardandoEscolha = false;
        etapaAtual = novaEtapa;
        falasAtuais = novasFalas;
        indiceFala = 0;
        if (painelEscolha != null) painelEscolha.SetActive(false);

        if (falasAtuais != null && falasAtuais.Count > 0)
        {
            if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
            MostrarFala();
        }
        else FinalizarDialogo();
    }

    private void AplicarExpressoes(FalaDeclaracao fala)
    {
        if (!string.IsNullOrWhiteSpace(fala.expressaoMyke))
        {
            ExpressaoPersonagem nova = EncontrarExpressao(myke, fala.expressaoMyke);
            if (nova != null) DefinirExpressao("Myke", nova);
        }
        if (!string.IsNullOrWhiteSpace(fala.expressaoPaxton))
        {
            ExpressaoPersonagem nova = EncontrarExpressao(paxton, fala.expressaoPaxton);
            if (nova != null) DefinirExpressao("Paxton", nova);
        }
    }

   private void DefinirExpressao(string nome, ExpressaoPersonagem nova)
{
    ExpressaoPersonagem anterior = ObterExpressaoAtual(nome);

    if (nova == null || anterior == nova)
        return;

    if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
        expressaoAtualMyke = nova;
    else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
        expressaoAtualPaxton = nova;

    RestaurarExpressao(nome);
}
    private ExpressaoPersonagem EncontrarExpressao(Personagem personagem, string nomeExpressao)
    {
        if (personagem == null || personagem.expressoes == null || string.IsNullOrWhiteSpace(nomeExpressao)) return null;
        foreach (ExpressaoPersonagem expressao in personagem.expressoes)
            if (expressao != null && nomeExpressao.Equals(expressao.nomeExpressao, System.StringComparison.OrdinalIgnoreCase))
                return expressao;
        Debug.LogWarning("Expressão '" + nomeExpressao + "' não encontrada para " + personagem.nome + ".");
        return null;
    }

    private ExpressaoPersonagem PrimeiraExpressao(Personagem personagem)
    {
        if (personagem == null || personagem.expressoes == null || personagem.expressoes.Count == 0) return null;
        return personagem.expressoes[0];
    }

    private ExpressaoPersonagem ObterExpressaoAtual(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return null;
        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase)) return expressaoAtualMyke;
        if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase)) return expressaoAtualPaxton;
        return null;
    }

    private void RestaurarExpressao(string nome)
    {
        Image imagem = ObterImagem(nome);
        ExpressaoPersonagem expressao = ObterExpressaoAtual(nome);
        if (imagem != null && expressao != null && expressao.bocaFechada != null)
            imagem.sprite = expressao.bocaFechada;
    }

    private Image ObterImagem(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return null;
        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase)) return imagemMyke;
        if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase)) return imagemPaxton;
        return null;
    }

    private void AplicarMovimentos(FalaDeclaracao fala)
    {
        if (fala.moverMyke && imagemMyke != null)
        {
            Vector2 destino = posicaoOriginalMyke + new Vector2(fala.deslocamentoXMyke, fala.deslocamentoYMyke);
            if (coroutineMovimentoMyke != null) StopCoroutine(coroutineMovimentoMyke);
            coroutineMovimentoMyke = StartCoroutine(MoverPersonagem(true, destino, fala.duracaoMovimentoMyke));
        }

        if (fala.moverPaxton && imagemPaxton != null)
        {
            Vector2 destino = posicaoOriginalPaxton + new Vector2(fala.deslocamentoXPaxton, fala.deslocamentoYPaxton);
            if (coroutineMovimentoPaxton != null) StopCoroutine(coroutineMovimentoPaxton);
            coroutineMovimentoPaxton = StartCoroutine(MoverPersonagem(false, destino, fala.duracaoMovimentoPaxton));
        }
    }

    private IEnumerator MoverPersonagem(bool moverMyke, Vector2 destino, float duracao)
    {
        Vector2 inicio = moverMyke ? posicaoBaseMyke : posicaoBasePaxton;
        if (duracao <= 0f) { DefinirPosicaoBase(moverMyke, destino); yield break; }
        float tempo = 0f;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            DefinirPosicaoBase(moverMyke, Vector2.Lerp(inicio, destino, Mathf.Clamp01(tempo / duracao)));
            yield return null;
        }
        DefinirPosicaoBase(moverMyke, destino);
    }

    private void DefinirPosicaoBase(bool moverMyke, Vector2 posicao)
    {
        if (moverMyke) posicaoBaseMyke = posicao;
        else posicaoBasePaxton = posicao;
    }

    private void IniciarPulinhoLetra(string nome)
    {
        if (nome.Equals("Myke", System.StringComparison.OrdinalIgnoreCase))
        {
            if (coroutinePuloLetraMyke != null) StopCoroutine(coroutinePuloLetraMyke);
            coroutinePuloLetraMyke = StartCoroutine(AnimarPulinho(nome, alturaPuloLetra, duracaoPuloLetra, false));
        }
        else if (nome.Equals("Paxton", System.StringComparison.OrdinalIgnoreCase))
        {
            if (coroutinePuloLetraPaxton != null) StopCoroutine(coroutinePuloLetraPaxton);
            coroutinePuloLetraPaxton = StartCoroutine(AnimarPulinho(nome, alturaPuloLetra, duracaoPuloLetra, false));
        }
    }

    private IEnumerator AnimarPulinho(string nome, float altura, float duracao, bool aoMudarExpressao)
    {
        duracao = Mathf.Max(0.01f, duracao);
        float tempo = 0f;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float valor = Mathf.Sin(Mathf.Clamp01(tempo / duracao) * Mathf.PI) * altura;
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
    }

    private void FinalizarDialogo()
    {
        finalizado = true;
        escrevendo = false;
        aguardandoEscolha = false;
        if (painelBalaoFala != null) painelBalaoFala.SetActive(false);
        if (painelEscolha != null) painelEscolha.SetActive(false);
        if (!string.IsNullOrWhiteSpace(nomeProximaCena)) SceneManager.LoadScene(nomeProximaCena);
    }

    public void ReiniciarDialogo()
    {
        PararCorrotinas();
        posicaoBaseMyke = posicaoOriginalMyke;
        posicaoBasePaxton = posicaoOriginalPaxton;
        puloLetraMyke = puloLetraPaxton = 0f;
        puloExpressaoMyke = puloExpressaoPaxton = 0f;
        expressaoAtualMyke = PrimeiraExpressao(myke);
        expressaoAtualPaxton = PrimeiraExpressao(paxton);
        RestaurarExpressao("Myke");
        RestaurarExpressao("Paxton");

        finalizado = false;
        escrevendo = false;
        aguardandoEscolha = false;
        etapaAtual = Etapa.AntesDaEscolha;
        falasAtuais = falasAntesDaEscolha;
        indiceFala = 0;
        if (painelEscolha != null) painelEscolha.SetActive(false);
        if (painelBalaoFala != null) painelBalaoFala.SetActive(true);
        if (falasAtuais != null && falasAtuais.Count > 0) MostrarFala();
        else MostrarEscolha();
    }

    private void PararCorrotinas()
    {
        if (coroutineDigitacao != null) StopCoroutine(coroutineDigitacao);
        if (coroutinePuloLetraMyke != null) StopCoroutine(coroutinePuloLetraMyke);
        if (coroutinePuloLetraPaxton != null) StopCoroutine(coroutinePuloLetraPaxton);
        if (coroutinePuloExpressaoMyke != null) StopCoroutine(coroutinePuloExpressaoMyke);
        if (coroutinePuloExpressaoPaxton != null) StopCoroutine(coroutinePuloExpressaoPaxton);
        if (coroutineMovimentoMyke != null) StopCoroutine(coroutineMovimentoMyke);
        if (coroutineMovimentoPaxton != null) StopCoroutine(coroutineMovimentoPaxton);
    }

    private void OnDestroy()
    {
        if (botaoAceitar != null) botaoAceitar.onClick.RemoveListener(AceitarDeclaracao);
        if (botaoRecusar != null) botaoRecusar.onClick.RemoveListener(RecusarDeclaracao);
    }
}
