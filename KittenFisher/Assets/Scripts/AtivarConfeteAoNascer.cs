using UnityEngine;

public class AtivarConfeteAoNascer : MonoBehaviour
{
    private ParticleSystem particula;

    void Awake()
    {
        // Pega a referência logo no início
        particula = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        if (particula != null)
        {
            particula.Clear(); // Limpa resíduos antigos
            particula.Play();  // Força o estouro dos confetes
            Debug.Log("🎉 Confete ativado via OnEnable com sucesso!");
        }
        else
        {
            // Procura nos filhos caso o script tenha sido colocado no objeto pai por engano
            particula = GetComponentInChildren<ParticleSystem>();
            if (particula != null)
            {
                particula.Clear();
                particula.Play();
            }
        }
    }
}