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
    [SerializeField] private Image blackScreen;
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
    private CanvasGroup canvasGroup;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        InitializeSingleton();
        ConfigureCanvas();
    }

    private void Start()
    {
        InitializeBlackScreen();
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

    private void ConfigureCanvas()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 9999; // Valor muy alto para estar sobre todo
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Añadir CanvasGroup para controlar la interactividad
            canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false; // Permite clicks a través del canvas
        }
    }

    private void InitializeBlackScreen()
    {
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(false);
            
            // Configurar color semi-transparente
            Color c = blackScreen.color;
            c.a = 0.7f; // 70% de opacidad
            blackScreen.color = c;
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
            
            if (blackScreen != null)
            {
                blackScreen.gameObject.SetActive(false);
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

            if (blackScreen == null) yield break;

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
            yield return ToggleBlackScreen(true);
            
            float blinkDuration = Random.Range(minBlinkDuration, maxBlinkDuration);
            yield return new WaitForSeconds(blinkDuration);
            
            yield return ToggleBlackScreen(false);
            
            if (i < rapidBlinks - 1)
            {
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
        }
    }

    private IEnumerator ExecuteSingleBlink()
    {
        yield return ToggleBlackScreen(true);
        
        float blinkDuration = Random.Range(minBlinkDuration, maxBlinkDuration);
        yield return new WaitForSeconds(blinkDuration);
        
        yield return ToggleBlackScreen(false);
    }

    private IEnumerator ToggleBlackScreen(bool state)
    {
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(state);
        }
        yield return null;
    }
    #endregion
}