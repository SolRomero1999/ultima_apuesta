using UnityEngine;

public class BartenderController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;  // Render del sprite
    [SerializeField] private Animator animator;  // Referencia al Animator

    [Header("Estado Inicial del Bartender")]
    [SerializeField] private Sprite initialSprite;  // Sprite inicial

    [Header("Estado Alternativo del Bartender")]
    [SerializeField] private Sprite alternateSprite;  // Segundo sprite

    private bool isAlternateState = false;  // Estado actual

    private void Start()
    {
        if (GameManager.instance != null && GameManager.instance.IsPlayerJudge())
        {
            SetAlternateState();
        }
        else
        {
            SetInitialState();
        }
    }

    public void SetInitialState()
    {
        spriteRenderer.sprite = initialSprite;
        isAlternateState = false;

        // Actualizar el parámetro del Animator para cambiar a la animación original
        if (animator != null)
        {
            animator.SetBool("IsAlternativeState", false);
        }
    }

    public void SetAlternateState()
    {
        spriteRenderer.sprite = alternateSprite;
        isAlternateState = true;

        // Actualizar el parámetro del Animator para cambiar a la animación alternativa
        if (animator != null)
        {
            animator.SetBool("IsAlternativeState", true);
        }
    }

    public bool IsAlternateState()
    {
        return isAlternateState;
    }
}
