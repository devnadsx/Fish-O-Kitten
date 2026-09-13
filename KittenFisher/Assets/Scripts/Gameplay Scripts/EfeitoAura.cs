using System.Collections;
using UnityEngine;

public class EfeitoAura : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DestruirComFade(float duracaoFade)
    {
        StartCoroutine(RoutineFadeOut(duracaoFade));
    }

    IEnumerator RoutineFadeOut(float duracao)
    {
        if (spriteRenderer != null)
        {
            Color corInicial = spriteRenderer.color;
            float tempo = 0f;

            while (tempo < duracao)
            {
                tempo += Time.deltaTime;
                float alfa = Mathf.Lerp(corInicial.a, 0f, tempo / duracao);
                spriteRenderer.color = new Color(corInicial.r, corInicial.g, corInicial.b, alfa);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}