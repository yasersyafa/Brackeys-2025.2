using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class Reeling : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI wordDisplayText;
    
    [Header("Typing Settings")]
    [SerializeField] private List<string> wordPool = new List<string>()
    {
        "FISH", "CATCH", "REEL", "WATER", "BAIT", "HOOK", "CAST", "PULL", "TIDE", "DEEP"
    };
    
    [SerializeField] private float newWordDelay = 0.5f;
    
    // Typing System Components
    private TypingSystem typingSystem;
    private WordSpawner wordSpawner;
    private InputHandler inputHandler;
    
    // Events
    public static event Action<string> OnWordCompleted;
    public static event Action<char> OnCorrectLetter;
    public static event Action<char> OnIncorrectLetter;
    
    void Awake()
    {
        InitializeTypingSystem();
    }
    
    void Update()
    {
        inputHandler?.HandleInput();
    }
    
    void OnEnable()
    {
        Bait.OnCaughtFish += StartTypingGame;
        TypingSystem.OnWordCompleted += HandleWordCompleted;
        TypingSystem.OnCorrectLetter += HandleCorrectLetter;
        TypingSystem.OnIncorrectLetter += HandleIncorrectLetter;
    }
    
    void OnDisable()
    {
        Bait.OnCaughtFish -= StartTypingGame;
        TypingSystem.OnWordCompleted -= HandleWordCompleted;
        TypingSystem.OnCorrectLetter -= HandleCorrectLetter;
        TypingSystem.OnIncorrectLetter -= HandleIncorrectLetter;
    }
    
    private void InitializeTypingSystem()
    {
        // Initialize the modular typing system
        wordSpawner = new WordSpawner(wordPool);
        typingSystem = new TypingSystem();
        inputHandler = new InputHandler(typingSystem);
        
        // Connect the display system
        if (wordDisplayText != null)
        {
            typingSystem.SetDisplayText(wordDisplayText);
        }
    }
    
    private void StartTypingGame()
    {
        SpawnNewWord();
    }
    
    private void SpawnNewWord()
    {
        string newWord = wordSpawner.GetRandomWord();
        typingSystem.SetCurrentWord(newWord);
    }
    
    private void HandleWordCompleted(string completedWord)
    {
        OnWordCompleted?.Invoke(completedWord);
        
        // Spawn new word after delay
        Invoke(nameof(SpawnNewWord), newWordDelay);
    }
    
    private void HandleCorrectLetter(char letter)
    {
        OnCorrectLetter?.Invoke(letter);
    }
    
    private void HandleIncorrectLetter(char letter)
    {
        OnIncorrectLetter?.Invoke(letter);
    }
    
    // Public methods for external control
    public void SetWordPool(List<string> newWordPool)
    {
        wordSpawner?.SetWordPool(newWordPool);
    }
    
    public void AddWordToPool(string word)
    {
        wordSpawner?.AddWord(word);
    }
    
    public string GetCurrentWord()
    {
        return typingSystem?.GetCurrentWord() ?? "";
    }
    
    public float GetCompletionPercentage()
    {
        return typingSystem?.GetCompletionPercentage() ?? 0f;
    }
    
    // New methods for word cycle management
    public int GetAvailableWordsCount()
    {
        return wordSpawner?.GetAvailableWordsCount() ?? 0;
    }
    
    public int GetUsedWordsCount()
    {
        return wordSpawner?.GetUsedWordsCount() ?? 0;
    }
    
    public List<string> GetUsedWords()
    {
        return wordSpawner?.GetUsedWords() ?? new List<string>();
    }
    
    public List<string> GetAvailableWords()
    {
        return wordSpawner?.GetAvailableWords() ?? new List<string>();
    }
    
    public void ResetWordCycle()
    {
        wordSpawner?.ForceResetCycle();
    }
}

// Modular Word Spawner System
[Serializable]
public class WordSpawner
{
    private List<string> originalWordPool;
    private List<string> availableWords;
    private List<string> usedWords;
    
    public WordSpawner(List<string> initialWordPool)
    {
        originalWordPool = new List<string>(initialWordPool);
        ResetWordCycle();
    }
    
    public string GetRandomWord()
    {
        // If no available words, reset the cycle
        if (availableWords == null || availableWords.Count == 0)
        {
            Debug.Log("All words used! Resetting word cycle...");
            ResetWordCycle();
        }
        
        // If still no words after reset, return default
        if (availableWords.Count == 0)
        {
            return "DEFAULT";
        }
        
        // Get random word from available words
        int randomIndex = UnityEngine.Random.Range(0, availableWords.Count);
        string selectedWord = availableWords[randomIndex].ToUpper();
        
        // Move word from available to used
        availableWords.RemoveAt(randomIndex);
        usedWords.Add(selectedWord);
        
        Debug.Log($"Selected word: {selectedWord} | Available: {availableWords.Count} | Used: {usedWords.Count}");
        
        return selectedWord;
    }
    
    private void ResetWordCycle()
    {
        availableWords = new List<string>(originalWordPool);
        if (usedWords == null)
        {
            usedWords = new List<string>();
        }
        else
        {
            usedWords.Clear();
        }
    }
    
    public void SetWordPool(List<string> newWordPool)
    {
        originalWordPool = new List<string>(newWordPool);
        ResetWordCycle();
    }
    
    public void AddWord(string word)
    {
        if (!string.IsNullOrEmpty(word))
        {
            string upperWord = word.ToUpper();
            if (!originalWordPool.Contains(upperWord))
            {
                originalWordPool.Add(upperWord);
                // Add to available words if not already used in current cycle
                if (!usedWords.Contains(upperWord))
                {
                    availableWords.Add(upperWord);
                }
            }
        }
    }
    
    public void RemoveWord(string word)
    {
        if (!string.IsNullOrEmpty(word))
        {
            string upperWord = word.ToUpper();
            originalWordPool.Remove(upperWord);
            availableWords.Remove(upperWord);
            usedWords.Remove(upperWord);
        }
    }
    
    public int GetWordPoolCount()
    {
        return originalWordPool?.Count ?? 0;
    }
    
    public int GetAvailableWordsCount()
    {
        return availableWords?.Count ?? 0;
    }
    
    public int GetUsedWordsCount()
    {
        return usedWords?.Count ?? 0;
    }
    
    public List<string> GetUsedWords()
    {
        return new List<string>(usedWords);
    }
    
    public List<string> GetAvailableWords()
    {
        return new List<string>(availableWords);
    }
    
    public void ForceResetCycle()
    {
        ResetWordCycle();
    }
}

// Modular Typing System
[Serializable]
public class TypingSystem
{
    private string currentWord = "";
    private string remainingWord = "";
    private TextMeshProUGUI displayText;
    
    // Events
    public static event Action<string> OnWordCompleted;
    public static event Action<char> OnCorrectLetter;
    public static event Action<char> OnIncorrectLetter;
    
    public void SetDisplayText(TextMeshProUGUI textComponent)
    {
        displayText = textComponent;
    }
    
    public void SetCurrentWord(string word)
    {
        currentWord = word.ToUpper();
        remainingWord = currentWord;
        UpdateDisplay();
    }
    
    public bool ProcessLetter(char inputLetter)
    {
        if (string.IsNullOrEmpty(remainingWord))
        {
            return false;
        }
        
        char expectedLetter = remainingWord[0];
        char upperInputLetter = char.ToUpper(inputLetter);
        
        if (upperInputLetter == expectedLetter)
        {
            // Correct letter - remove it from remaining word
            remainingWord = remainingWord.Substring(1);
            OnCorrectLetter?.Invoke(upperInputLetter);
            UpdateDisplay();
            
            // Check if word is completed
            if (string.IsNullOrEmpty(remainingWord))
            {
                OnWordCompleted?.Invoke(currentWord);
            }
            
            return true;
        }
        else
        {
            // Incorrect letter
            OnIncorrectLetter?.Invoke(upperInputLetter);
            return false;
        }
    }
    
    private void UpdateDisplay()
    {
        if (displayText != null)
        {
            // Show completed part in different color/style and remaining part
            string completedPart = currentWord.Substring(0, currentWord.Length - remainingWord.Length);
            string remainingPart = remainingWord;
            
            // Using TextMeshPro rich text formatting
            displayText.text = $"<color=green>{completedPart}</color><color=white>{remainingPart}</color>";
        }
    }
    
    public string GetCurrentWord()
    {
        return currentWord;
    }
    
    public string GetRemainingWord()
    {
        return remainingWord;
    }
    
    public float GetCompletionPercentage()
    {
        if (string.IsNullOrEmpty(currentWord))
        {
            return 0f;
        }
        
        int completedLetters = currentWord.Length - remainingWord.Length;
        return (float)completedLetters / currentWord.Length;
    }
    
    public bool IsWordCompleted()
    {
        return string.IsNullOrEmpty(remainingWord);
    }
}

// Modular Input Handler
[Serializable]
public class InputHandler
{
    private TypingSystem typingSystem;
    
    public InputHandler(TypingSystem system)
    {
        typingSystem = system;
    }
    
    public void HandleInput()
    {
        // Handle keyboard input using Unity's Input System
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Check for letter inputs A-Z
        if (keyboard.aKey.wasPressedThisFrame) typingSystem.ProcessLetter('A');
        if (keyboard.bKey.wasPressedThisFrame) typingSystem.ProcessLetter('B');
        if (keyboard.cKey.wasPressedThisFrame) typingSystem.ProcessLetter('C');
        if (keyboard.dKey.wasPressedThisFrame) typingSystem.ProcessLetter('D');
        if (keyboard.eKey.wasPressedThisFrame) typingSystem.ProcessLetter('E');
        if (keyboard.fKey.wasPressedThisFrame) typingSystem.ProcessLetter('F');
        if (keyboard.gKey.wasPressedThisFrame) typingSystem.ProcessLetter('G');
        if (keyboard.hKey.wasPressedThisFrame) typingSystem.ProcessLetter('H');
        if (keyboard.iKey.wasPressedThisFrame) typingSystem.ProcessLetter('I');
        if (keyboard.jKey.wasPressedThisFrame) typingSystem.ProcessLetter('J');
        if (keyboard.kKey.wasPressedThisFrame) typingSystem.ProcessLetter('K');
        if (keyboard.lKey.wasPressedThisFrame) typingSystem.ProcessLetter('L');
        if (keyboard.mKey.wasPressedThisFrame) typingSystem.ProcessLetter('M');
        if (keyboard.nKey.wasPressedThisFrame) typingSystem.ProcessLetter('N');
        if (keyboard.oKey.wasPressedThisFrame) typingSystem.ProcessLetter('O');
        if (keyboard.pKey.wasPressedThisFrame) typingSystem.ProcessLetter('P');
        if (keyboard.qKey.wasPressedThisFrame) typingSystem.ProcessLetter('Q');
        if (keyboard.rKey.wasPressedThisFrame) typingSystem.ProcessLetter('R');
        if (keyboard.sKey.wasPressedThisFrame) typingSystem.ProcessLetter('S');
        if (keyboard.tKey.wasPressedThisFrame) typingSystem.ProcessLetter('T');
        if (keyboard.uKey.wasPressedThisFrame) typingSystem.ProcessLetter('U');
        if (keyboard.vKey.wasPressedThisFrame) typingSystem.ProcessLetter('V');
        if (keyboard.wKey.wasPressedThisFrame) typingSystem.ProcessLetter('W');
        if (keyboard.xKey.wasPressedThisFrame) typingSystem.ProcessLetter('X');
        if (keyboard.yKey.wasPressedThisFrame) typingSystem.ProcessLetter('Y');
        if (keyboard.zKey.wasPressedThisFrame) typingSystem.ProcessLetter('Z');
    }
}
