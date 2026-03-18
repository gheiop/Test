using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Islebound.Player;

namespace Islebound.Crafting
{
    public class WorkbenchUI : MonoBehaviour
    {
        public static WorkbenchUI Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject workbenchRoot;

        [Header("Header")]
        [SerializeField] private TMP_Text stationTitleText;

        [Header("Recipe List")]
        [SerializeField] private Transform recipeButtonContainer;
        [SerializeField] private CraftingRecipeButtonUI recipeButtonPrefab;

        [Header("Selected Recipe")]
        [SerializeField] private TMP_Text selectedRecipeNameText;
        [SerializeField] private TMP_Text selectedRecipeDescriptionText;
        [SerializeField] private TMP_Text selectedRecipeIngredientsText;
        [SerializeField] private TMP_Text selectedRecipeResultText;
        [SerializeField] private TMP_Text feedbackText;

        [Header("Buttons")]
        [SerializeField] private Button craftButton;
        [SerializeField] private Button closeButton;

        [Header("Behavior")]
        [SerializeField] private bool closeWithEscape = true;
        [SerializeField] private float autoRefreshInterval = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool debugLogs = false;

        private readonly List<CraftingRecipeButtonUI> spawnedButtons = new List<CraftingRecipeButtonUI>();

        private Workbench currentWorkbench;
        private CraftingRecipeData selectedRecipe;
        private float refreshTimer;
        private PlayerLook playerLook;

        public bool IsOpen => workbenchRoot != null && workbenchRoot.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            playerLook = FindFirstObjectByType<PlayerLook>();

            if (craftButton != null)
            {
                craftButton.onClick.AddListener(TryCraftSelected);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (workbenchRoot != null)
            {
                workbenchRoot.SetActive(false);
            }
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            if (closeWithEscape && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }

            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0f)
            {
                refreshTimer = autoRefreshInterval;
                RefreshView();
            }
        }

        public void Open(Workbench workbench)
        {
            if (workbench == null)
                return;

            currentWorkbench = workbench;
            selectedRecipe = null;
            refreshTimer = 0f;

            if (workbenchRoot != null)
            {
                workbenchRoot.SetActive(true);
            }

            if (stationTitleText != null)
            {
                stationTitleText.text = workbench.StationName;
            }

            SetCursorState(true);
            BuildRecipeButtons();
            SelectFirstRecipe();
            RefreshView();

            if (debugLogs)
            {
                Debug.Log($"[WorkbenchUI] Opened station: {workbench.StationName}");
            }
        }

        public void Close()
        {
            currentWorkbench = null;
            selectedRecipe = null;

            if (workbenchRoot != null)
            {
                workbenchRoot.SetActive(false);
            }

            if (feedbackText != null)
            {
                feedbackText.text = string.Empty;
            }

            SetCursorState(false);

            if (debugLogs)
            {
                Debug.Log("[WorkbenchUI] Closed.");
            }
        }

        private void SetCursorState(bool visible)
        {
            if (playerLook == null)
            {
                playerLook = FindFirstObjectByType<PlayerLook>();
            }

            if (playerLook != null)
            {
                playerLook.SetCursorVisible(visible);
            }
        }

        private void BuildRecipeButtons()
        {
            ClearRecipeButtons();

            if (currentWorkbench == null || recipeButtonContainer == null || recipeButtonPrefab == null)
                return;

            CraftingRecipeData[] recipes = currentWorkbench.Recipes;
            if (recipes == null)
                return;

            for (int i = 0; i < recipes.Length; i++)
            {
                CraftingRecipeData recipe = recipes[i];
                if (recipe == null)
                    continue;

                CraftingRecipeButtonUI button = Instantiate(recipeButtonPrefab, recipeButtonContainer);
                RectTransform rect = button.transform as RectTransform;
                if (rect != null)
                {
                    rect.localScale = Vector3.one;
                    rect.anchoredPosition3D = Vector3.zero;
                }

                button.Setup(recipe, SelectRecipe);
                spawnedButtons.Add(button);
            }
        }

        private void ClearRecipeButtons()
        {
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                if (spawnedButtons[i] != null)
                {
                    Destroy(spawnedButtons[i].gameObject);
                }
            }

            spawnedButtons.Clear();
        }

        private void SelectFirstRecipe()
        {
            if (currentWorkbench == null || currentWorkbench.Recipes == null || currentWorkbench.Recipes.Length == 0)
            {
                selectedRecipe = null;
                return;
            }

            for (int i = 0; i < currentWorkbench.Recipes.Length; i++)
            {
                if (currentWorkbench.Recipes[i] != null)
                {
                    selectedRecipe = currentWorkbench.Recipes[i];
                    return;
                }
            }

            selectedRecipe = null;
        }

        private void SelectRecipe(CraftingRecipeData recipe)
        {
            selectedRecipe = recipe;
            RefreshView();

            if (debugLogs && recipe != null)
            {
                Debug.Log($"[WorkbenchUI] Selected recipe: {recipe.DisplayName}");
            }
        }

        private void RefreshView()
        {
            RefreshButtons();
            RefreshSelectedRecipePanel();
        }

        private void RefreshButtons()
        {
            CraftingManager craftingManager = CraftingManager.Instance;

            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                CraftingRecipeButtonUI button = spawnedButtons[i];
                if (button == null || button.Recipe == null)
                    continue;

                bool canCraft = craftingManager != null && craftingManager.CanCraft(button.Recipe);
                bool selected = selectedRecipe == button.Recipe;
                button.RefreshVisual(canCraft, selected);
            }
        }

        private void RefreshSelectedRecipePanel()
        {
            if (selectedRecipe == null)
            {
                if (selectedRecipeNameText != null) selectedRecipeNameText.text = "No recipe selected";
                if (selectedRecipeDescriptionText != null) selectedRecipeDescriptionText.text = string.Empty;
                if (selectedRecipeIngredientsText != null) selectedRecipeIngredientsText.text = string.Empty;
                if (selectedRecipeResultText != null) selectedRecipeResultText.text = string.Empty;
                if (craftButton != null) craftButton.interactable = false;
                return;
            }

            if (selectedRecipeNameText != null)
            {
                selectedRecipeNameText.text = selectedRecipe.DisplayName;
            }

            if (selectedRecipeDescriptionText != null)
            {
                selectedRecipeDescriptionText.text = selectedRecipe.Description;
            }

            if (selectedRecipeResultText != null)
            {
                string outputName = selectedRecipe.OutputItem != null ? selectedRecipe.OutputItem.DisplayName : "Missing Output";
                selectedRecipeResultText.text = $"Result: {outputName} x{selectedRecipe.OutputAmount}";
            }

            if (selectedRecipeIngredientsText != null)
            {
                if (CraftingManager.Instance != null)
                {
                    selectedRecipeIngredientsText.text = CraftingManager.Instance.BuildIngredientsText(selectedRecipe);
                }
                else
                {
                    selectedRecipeIngredientsText.text = "CraftingManager missing.";
                }
            }

            bool canCraft = CraftingManager.Instance != null && CraftingManager.Instance.CanCraft(selectedRecipe);

            if (craftButton != null)
            {
                craftButton.interactable = canCraft;
            }
        }

        private void TryCraftSelected()
        {
            if (selectedRecipe == null)
                return;

            if (CraftingManager.Instance == null)
            {
                if (feedbackText != null)
                {
                    feedbackText.text = "CraftingManager missing.";
                }
                return;
            }

            bool crafted = CraftingManager.Instance.Craft(selectedRecipe);

            if (feedbackText != null)
            {
                feedbackText.text = crafted
                    ? $"Crafted: {selectedRecipe.DisplayName}"
                    : $"Cannot craft: {selectedRecipe.DisplayName}";
            }

            RefreshView();
        }
    }
}