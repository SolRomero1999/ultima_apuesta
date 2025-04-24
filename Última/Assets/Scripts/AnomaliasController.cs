using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnomaliasController : MonoBehaviour
{
    #region Singleton Pattern
    public static AnomaliasController Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("Configuración de Parpadeo")]
    [SerializeField] private Image blinkPanel; // Cambiado de SpriteRenderer a Image
    [SerializeField, Range(0.05f, 1f)] private float minBlinkInterval = 0.1f;
    [SerializeField, Range(1f, 5f)] private float maxBlinkInterval = 3f;
    [SerializeField, Range(0.01f, 0.5f)] private float minBlinkDuration = 0.05f;
    [SerializeField, Range(0.1f, 1f)] private float maxBlinkDuration = 0.3f;
    [SerializeField, Range(0f, 1f)] private float chanceForRapidBlinks = 0.3f;
    [SerializeField, Range(1, 10)] private int maxAnomalias = 1;
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
        InitializeBlinkPanel();
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

    private void InitializeBlinkPanel()
    {
        if (blinkPanel != null)
        {
            blinkPanel.enabled = false;
            
            // Configurar color semi-transparente
            Color c = blinkPanel.color;
            c.a = 0.7f;
            blinkPanel.color = c;
            
            // Deshabilitar Raycast Target para no bloquear interacciones
            blinkPanel.raycastTarget = false; // ← Esta es la línea clave
            
            // Configurar para cubrir toda la pantalla
            RectTransform rt = blinkPanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
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
            
            if (blinkPanel != null)
            {
                blinkPanel.enabled = false;
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

            if (blinkPanel == null) yield break;

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
            yield return ToggleBlinkPanel(true);
            
            float blinkDuration = Random.Range(minBlinkDuration, maxBlinkDuration);
            yield return new WaitForSeconds(blinkDuration);
            
            yield return ToggleBlinkPanel(false);
            
            if (i < rapidBlinks - 1)
            {
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
        }
    }

    private IEnumerator ExecuteSingleBlink()
    {
        yield return ToggleBlinkPanel(true);
        
        float blinkDuration = Random.Range(minBlinkDuration, maxBlinkDuration);
        yield return new WaitForSeconds(blinkDuration);
        
        yield return ToggleBlinkPanel(false);
    }

    private IEnumerator ToggleBlinkPanel(bool state)
    {
        if (blinkPanel != null)
        {
            blinkPanel.enabled = state;
        }
        yield return null;
    }
    #endregion
}