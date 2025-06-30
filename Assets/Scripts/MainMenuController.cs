using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;
    
    // UI Elements
    private Button startButton;
    private Button endButton;
    private Label headerLabel;
    
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float buttonClickDelay = 0.1f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip menuMusic;
    
    private void Start()
    {
        InitializeUI();
        SetupEventHandlers();
        PlayMenuMusic();
    }
    
    private void InitializeUI()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        if (uiDocument == null)
        {
            Debug.LogError("MainMenuController: UIDocument component not found!");
            return;
        }
        
        var root = uiDocument.rootVisualElement;
        
        // Get UI elements by name
        startButton = root.Q<Button>("Start");
        endButton = root.Q<Button>("End");
        headerLabel = root.Q<Label>("Header");
        
        if (startButton == null || endButton == null || headerLabel == null)
        {
            Debug.LogError("MainMenuController: One or more UI elements not found!");
            return;
        }
        
        // Set initial button states
        startButton.focusable = true;
        endButton.focusable = true;
        
        // Set focus to start button
        startButton.Focus();
    }
    
    private void SetupEventHandlers()
    {
        if (startButton != null)
        {
            startButton.clicked += OnStartButtonClicked;
            startButton.RegisterCallback<KeyDownEvent>(OnStartButtonKeyDown);
        }
        
        if (endButton != null)
        {
            endButton.clicked += OnEndButtonClicked;
            endButton.RegisterCallback<KeyDownEvent>(OnEndButtonKeyDown);
        }
    }
    
    private void OnStartButtonClicked()
    {
        PlayButtonClickSound();
        DisableButtons();
        
        // Add a small delay for better UX
        Invoke(nameof(LoadGameScene), buttonClickDelay);
    }
    
    private void OnStartButtonKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            OnStartButtonClicked();
        }
    }
    
    private void OnEndButtonClicked()
    {
        PlayButtonClickSound();
        DisableButtons();
        
        // Add a small delay for better UX
        Invoke(nameof(QuitGame), buttonClickDelay);
    }
    
    private void OnEndButtonKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            OnEndButtonClicked();
        }
    }
    
    private void LoadGameScene()
    {
        Debug.Log("Loading game scene: " + gameSceneName);
        
        // Debug: List all available scenes in build settings
        Debug.Log("Available scenes in build settings:");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"Scene {i}: {sceneName} (Path: {scenePath})");
        }
        
        // Try to load by name first
        Scene scene = SceneManager.GetSceneByName(gameSceneName);
        if (scene.IsValid())
        {
            SceneManager.LoadScene(gameSceneName);
            return;
        }
        
        // Try to load by build index if name doesn't work
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (sceneName.Equals(gameSceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"Found scene '{gameSceneName}' at build index {i}, loading...");
                SceneManager.LoadScene(i);
                return;
            }
        }
        
        // If still not found, try to load the first non-main-menu scene
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (!sceneName.ToLower().Contains("menu") && !sceneName.ToLower().Contains("main"))
            {
                Debug.Log($"Loading fallback scene: {sceneName} at index {i}");
                SceneManager.LoadScene(i);
                return;
            }
        }
        
        // Last resort: load scene at index 1 (assuming index 0 is main menu)
        if (SceneManager.sceneCountInBuildSettings > 1)
        {
            Debug.LogWarning($"Scene '{gameSceneName}' not found. Loading scene at index 1 as fallback.");
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.LogError($"Scene '{gameSceneName}' not found and no fallback scenes available!");
        }
    }
    
    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void DisableButtons()
    {
        if (startButton != null)
        {
            startButton.SetEnabled(false);
        }
        
        if (endButton != null)
        {
            endButton.SetEnabled(false);
        }
    }
    
    private void PlayButtonClickSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    private void PlayMenuMusic()
    {
        if (audioSource != null && menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event handlers
        if (startButton != null)
        {
            startButton.clicked -= OnStartButtonClicked;
            startButton.UnregisterCallback<KeyDownEvent>(OnStartButtonKeyDown);
        }
        
        if (endButton != null)
        {
            endButton.clicked -= OnEndButtonClicked;
            endButton.UnregisterCallback<KeyDownEvent>(OnEndButtonKeyDown);
        }
    }
    
    // Public methods for external access
    public void SetGameSceneName(string sceneName)
    {
        gameSceneName = sceneName;
    }
    
    public void SetButtonClickDelay(float delay)
    {
        buttonClickDelay = delay;
    }
}
