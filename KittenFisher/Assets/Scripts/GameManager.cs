using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public int itensTotais;
    private int itensEncontrados = 0;
    public UnityEvent aoVencer;

    public void RegistrarItemEncontrado()
    {
        itensEncontrados++;
        Debug.Log($"Itens: {itensEncontrados} / {itensTotais}");

        if (itensEncontrados >= itensTotais)
        {
            Debug.Log("Vitória!");
            aoVencer.Invoke();
        }
    }
}