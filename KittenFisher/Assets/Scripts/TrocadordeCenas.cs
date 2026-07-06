using UnityEngine;
using UnityEngine.SceneManagement;

public class TrocadorDeCenas : MonoBehaviour
{

    public void IrParaCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }


    // Aqui é pra caso vc quiser ter um de kitar
    public void SairDoJogo()
    {
        // Mostra uma mensagem no console para você saber que funcionou (Pode deletar se quiser)
        Debug.Log("But kitou..");

        Application.Quit();
    }
}