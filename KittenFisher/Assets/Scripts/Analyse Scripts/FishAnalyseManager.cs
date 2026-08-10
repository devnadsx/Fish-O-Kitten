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
            DialogoManager.Instance.AdicionarFala("Time to test and taste these fish! Select one from the table.", false);
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
            DialogoManager.Instance.LimparDialogo();

            if (peixeAtualEhVenenoso)
            {
                DialogoManager.Instance.AdicionarFala($"Ugh! The {peixeAtualNome} was super poisonous!", false);
                DialogoManager.Instance.AdicionarFala("Gosh! I'm glad I was careful.., otherwise, I would definitely be dead..", false);
                DialogoManager.Instance.AdicionarFala("Myke breathe a little, trying to not vomit again. He wrote down while coughing a lot.", true);
                DialogoManager.Instance.AdicionarFala("Okay, time to another one..", false);
            }
            else
            {
                DialogoManager.Instance.AdicionarFala($"Mmmph! The {peixeAtualNome} is delicious and perfectly safe.", false);
                DialogoManager.Instance.AdicionarFala("Myke finish tasting and wrote down about the fish.", true);
                DialogoManager.Instance.AdicionarFala("Okay, I need to finish these fish..", false);
            }
        }

        VerificarFimDaAnalise();
    }

    public void OnSkillCheckFalha()
    {
        if (peixeAtualObjeto != null) peixeAtualObjeto.SetActive(false);
        peixesProcessados++;

        if (DialogoManager.Instance != null)
        {
            DialogoManager.Instance.LimparDialogo();

            DialogoManager.Instance.AdicionarFala($"Ughh! {peixeAtualNome} definity not safe!-", false);
            DialogoManager.Instance.AdicionarFala("Myke runs to an empty bucket and finish vomiting.", true);
            DialogoManager.Instance.AdicionarFala("He wrote down, with a sickened face.", true);
            DialogoManager.Instance.AdicionarFala("Okay, Let me continue this..", false);
        }

        VerificarFimDaAnalise();
    }

    void VerificarFimDaAnalise()
    {
        if (peixesProcessados >= totalPeixesNaMesa)
        {
            StartCoroutine(AguardarFimDosDialogosEAvancar());
        }
    }

    IEnumerator AguardarFimDosDialogosEAvancar()
    {
        // Espera o jogador terminar de ler todas as falas antes de mudar de cena
        yield return new WaitForSeconds(0.5f);

        while (DialogoManager.Instance != null && DialogoManager.Instance.TemFalasPendentes())
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