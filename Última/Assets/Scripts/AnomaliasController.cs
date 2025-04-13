using UnityEngine;
using System.Collections;

public class AnomaliasController : MonoBehaviour
{
    #region Singleton Pattern
    public static AnomaliasController Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("Configuración de Parpadeo")]
    [SerializeField] private SpriteRenderer blinkSprite;
    [SerializeField, Range(0.05f, 1f)] private float minBlinkInterval = 0.1f;
    [SerializeField, Range(1f, 5f)] private float maxBlinkInterval = 3f;
    [SerializeField, Range(0.01f, 0.5f)] private float minBlinkDuration = 0.05f;
    [SerializeField, Range(0.1f, 1f)] private float maxBlinkDuration = 0.3f;
    [SerializeField, Range(0f, 1f)] private float chanceForRapidBlinks = 0.3f;
    [SerializeField, Range(1, 10)] private int maxAnomalias = 4;
    #endregion

    #region Private Variables
    private int anomaliasCount = 0;
    private Coroutine blinkingCoroutine;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeBlinkSprite();
    }

    private void OnDestroy()
    {
        CleanUpCoroutines();
    }
    #endregion

    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeBlinkSprite()
    {
        if (blinkSprite != null)
        {
            blinkSprite.enabled = false;
            
            // Configurar color semi-transparente
            Color c = blinkSprite.color;
            c.a = 0.7f; // 70% de opacidad
            blinkSprite.color = c;
            
            // Mantiene la posición y escala que hayas asignado en el editor
        }
    }
    #endregion

    #region Public Methods
    public void AddAnomalia()
    {
        if (anomaliasCount < maxAnomalias)
        {
            anomaliasCount++;
            UpdateAnomaliasState();
        }
    }

    public void RemoveAnomalia()
    {
        if (anomaliasCount > 0)
        {
            anomaliasCount--;
            UpdateAnomaliasState();
        }
    }

    public int GetAnomaliasCount() => anomaliasCount;
    #endregion

    #region Private Methods
    private void UpdateAnomaliasState()
    {
        if (anomaliasCount > 0)
        {
            if (blinkingCoroutine == null)
            {
                blinkingCoroutine = StartCoroutine(BlinkingLightsRoutine());
            }
        }
        else
        {
            CleanUpCoroutines();
        }
    }

    private void CleanUpCoroutines()
    {
        if (blinkingCoroutine != null)
        {
            StopCoroutine(blinkingCoroutine);
            blinkingCoroutine = null;
            
            if (blinkSprite != null)
            {
                blinkSprite.enabled = false;
            }
        }
    }
    #endregion

    #region Coroutines
    private IEnumerator BlinkingLightsRoutine()
    {
        while (anomaliasCount > 0)
        {
            float waitTime = Random.Range(
                minBlinkInterval, 
                Mathf.Max(minBlinkInterval, maxBlinkInterval / anomaliasCount)
            );
            yield return new WaitForSeconds(waitTime);

            if (blinkSprite == null) yield break;

            if (Random.value < chanceForRapidBlinks)
            {
                yield return ExecuteRapidBlinks();
            }
            else
            {
                yield return ExecuteSingleBlink();
            }
        }
    }

    private IEnumerator ExecuteRapidBlinks()
    {
        int rapidBlinks = Random.Range(2, 5);
        for (int i = 0; i < rapidBlinks; i++)
        {
            yield return ToggleBlinkSprite(true);
            
            float blinkDuration = Random.Range(minBlinkDuration, maxBlinkDuration);
            yield return new WaitForSeconds(blinkDuration);
            
            yield return ToggleBlinkSprite(false);
            
            if (i < rapidBlinks - 1)
            {
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
        }
    }

    private IEnumerator ExecuteSingleBlink()
    {
        yield return ToggleBlinkSprite(true);
        
        float blinkDuration = Random.Range(minBlinkDuration, maxBlinkDuration);
        yield return new WaitForSeconds(blinkDuration);
        
        yield return ToggleBlinkSprite(false);
    }

    private IEnumerator ToggleBlinkSprite(bool state)
    {
        if (blinkSprite != null)
        {
            blinkSprite.enabled = state;
        }
        yield return null;
    }
    #endregion
}