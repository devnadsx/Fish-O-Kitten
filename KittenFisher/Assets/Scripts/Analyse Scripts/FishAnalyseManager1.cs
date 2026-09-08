using System.Collections;
using UnityEngine;

public class FishAnalyseManager : MonoBehaviour
{
    public static FishAnalyseManager Instance;

    [Header("Referência da UI (Objeto com o script de UI)")]
    public MonoBehaviour uiManagerScript; // Aceita o script de UI dinamicamente

    [Header("Controle de Peixes")]
    public int totalPeixesNaMesa = 3;
    private int peixesProcessados = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void IniciarTesteDoPeixe(string nomePeixe, bool ehVenenoso, GameObject peixeGameObject)
    {
        string mensagemAnalise = ehVenenoso
            ? $"Hmm... This {nomePeixe} looks dangerous and toxic!"
            : $"This {nomePeixe} seems completely safe to eat.";

        // Chama o método "MostrarFala" no script de UI sem precisar declarar o tipo direto
        if (uiManagerScript != null)
        {
            uiManagerScript.SendMessage("MostrarFala", mensagemAnalise, SendMessageOptions.DontRequireReceiver);
        }

        if (peixeGameObject != null)
        {
            peixeGameObject.SetActive(false);
        }

        peixesProcessados++;
        VerificarFimDaAnalise();
    }

    void VerificarFimDaAnalise()
    {
        if (peixesProcessados >= totalPeixesNaMesa && uiManagerScript != null)
        {
            uiManagerScript.SendMessage("MostrarFala", "All fish analyzed! Moving on...", SendMessageOptions.DontRequireReceiver);
            StartCoroutine(AguardarETrocarCena());
        }
    }

    IEnumerator AguardarETrocarCena()
    {
        yield return new WaitForSeconds(2.5f);
        if (uiManagerScript != null)
        {
            uiManagerScript.SendMessage("FinalizarAnalyseETrocarCena", SendMessageOptions.DontRequireReceiver);
        }
    }
}