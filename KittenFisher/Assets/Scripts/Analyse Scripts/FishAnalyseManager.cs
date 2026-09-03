using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FishAnalyseManager : MonoBehaviour
{
    public static FishAnalyseManager Instance;

    [Header("Aviso de Controles (Canva)")]
    public GameObject imagemAvisoControles;

    [Header("Configuração da Cena")]
    public string nomeProximaFase = "Game2";
    public int totalPeixesNaMesa = 3;
    private int peixesProcessados = 0;

    // Guarda o peixe em teste no momento
    private GameObject peixeAtualObjeto;
    private string peixeAtualNome;
    private bool peixeAtualEhVenenoso;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (imagemAvisoControles != null)
        {
            imagemAvisoControles.SetActive(true);
        }

        if (DialogoManager.Instance != null)
        {
            List<LineDialogo> falaInicial = new List<LineDialogo>()
            {
                new LineDialogo
                {
                    nomeQuemFala = "Myke",
                    texto = "Time to taste and write down these fish! Which one should i start?",
                    ehNarracao = false,
                    posicao = PosicaoPersonagem.Centro
                }
            };

            DialogoManager.Instance.IniciarSequenciaDialogo(falaInicial);
        }
    }

    void Update()
    {
        // Se o aviso de controles estiver ativo e o jogador apertar Enter, esconde ele
        if (imagemAvisoControles != null && imagemAvisoControles.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                imagemAvisoControles.SetActive(false);
            }
        }
    }

    public void IniciarTesteDoPeixe(string nome, bool ehVenenoso, GameObject objetoPeixe)
    {
        if (imagemAvisoControles != null && imagemAvisoControles.activeSelf)
        {
            imagemAvisoControles.SetActive(false);
        }

        peixeAtualNome = nome;
        peixeAtualEhVenenoso = ehVenenoso;
        peixeAtualObjeto = objetoPeixe;

        if (DialogoManager.Instance != null)
        {
            DialogoManager.Instance.FecharDialogo();
        }

        if (ehVenenoso)
        {
            if (SkillCheckManager.Instance != null)
            {
                SkillCheckManager.Instance.IniciarSequenciaSkillCheck();
            }
        }
        else
        {
            OnSkillCheckSucesso();
        }
    }

    public void OnSkillCheckSucesso()
    {
        if (peixeAtualObjeto != null) peixeAtualObjeto.SetActive(false);
        peixesProcessados++;

        if (DialogoManager.Instance != null)
        {
            List<LineDialogo> sequenciaFalas = new List<LineDialogo>();

            if (peixeAtualEhVenenoso)
            {
                sequenciaFalas.Add(new LineDialogo { nomeQuemFala = "Myke", texto = $"Ugh! The {peixeAtualNome} was super poisonous!", ehNarracao = false, posicao = PosicaoPersonagem.Centro });
                sequenciaFalas.Add(new LineDialogo { nomeQuemFala = "Myke", texto = "Gosh! I'm glad I was careful.., otherwise, I would definitely be dead..", ehNarracao = false, posicao = PosicaoPersonagem.Centro });
                sequenciaFalas.Add(new LineDialogo { nomeQuemFala = "Narrador", texto = "Myke breathe a little, trying to not vomit again. He wrote down while coughing a lot.", ehNarracao = true, posicao = PosicaoPersonagem.Centro });

                if (peixesProcessados < totalPeixesNaMesa)
                {
                    sequenciaFalas.Add(new LineDialogo { nomeQuemFala = "Myke", texto = "Okay, time to another one..", ehNarracao = false, posicao = PosicaoPersonagem.Centro });
                }
            }
            else
            {
                sequenciaFalas.Add(new LineDialogo { nomeQuemFala = "Myke", texto = $"Mmmph! The {peixeAtualNome} is delicious and perfectly safe.", ehNarracao = false, posicao = PosicaoPersonagem.Centro });
                sequenciaFalas.Add(new LineDialogo { nomeQuemFala = "Narrador", texto = "Myke finish tasting and wrote down about the fish.", ehNarracao = true, posicao = PosicaoPersonagem.Centro });

                if (peixesProcessados < totalPeixesNaMesa)
                {
                    sequenciaFalas.Add(new LineDialogo { nomeQuemFala = "Myke", texto = "Okay, I need to finish these fish..", ehNarracao = false, posicao = PosicaoPersonagem.Centro });
                }
            }

            DialogoManager.Instance.IniciarSequenciaDialogo(sequenciaFalas);
        }

        VerificarFimDaAnalise();
    }

    public void OnSkillCheckFalha()
    {
        if (peixeAtualObjeto != null) peixeAtualObjeto.SetActive(false);
        peixesProcessados++;

        if (DialogoManager.Instance != null)
        {
            List<LineDialogo> sequenciaFalas = new List<LineDialogo>()
            {
                new LineDialogo { nomeQuemFala = "Myke", texto = $"Ughh! {peixeAtualNome} definity not safe!-", ehNarracao = false, posicao = PosicaoPersonagem.Centro },
                new LineDialogo { nomeQuemFala = "Narrador", texto = "Myke runs to an empty bucket and finish vomiting.", ehNarracao = true, posicao = PosicaoPersonagem.Centro },
                new LineDialogo { nomeQuemFala = "Narrador", texto = "He wrote down, with a sickened face.", ehNarracao = true, posicao = PosicaoPersonagem.Centro }
            };

            if (peixesProcessados < totalPeixesNaMesa)
            {
                sequenciaFalas.Add(new LineDialogo { nomeQuemFala = "Myke", texto = "Okay, Let me continue this..", ehNarracao = false, posicao = PosicaoPersonagem.Centro });
            }

            DialogoManager.Instance.IniciarSequenciaDialogo(sequenciaFalas);
        }

        VerificarFimDaAnalise();
    }

    void VerificarFimDaAnalise()
    {
        if (peixesProcessados >= totalPeixesNaMesa)
        {
            if (DialogoManager.Instance != null)
            {
                List<LineDialogo> falaFinal = new List<LineDialogo>()
                {
                    new LineDialogo { nomeQuemFala = "Myke", texto = "Alright! I've finished analyzing all the fish. Time to move on!", ehNarracao = false, posicao = PosicaoPersonagem.Centro }
                };

                DialogoManager.Instance.IniciarSequenciaDialogo(falaFinal);
            }

            StartCoroutine(AguardarFimDosDialogosEAvancar());
        }
    }

    IEnumerator AguardarFimDosDialogosEAvancar()
    {
        yield return new WaitForSeconds(0.5f);

        // Aguarda enquanto o painel/caixa do DialogoManager estiver visível na tela
        while (DialogoManager.Instance != null && DialogoManager.Instance.gameObject.activeInHierarchy)
        {
            yield return null;
        }

        IrParaProximaFase();
    }

    public void PularAnalise()
    {
        IrParaProximaFase();
    }

    public void IrParaProximaFase()
    {
        SceneManager.LoadScene(nomeProximaFase);
    }
}