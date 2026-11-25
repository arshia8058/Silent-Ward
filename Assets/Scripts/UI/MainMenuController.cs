using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SilentWard.Core;

namespace SilentWard.UI
{
    /// <summary>Main menu (GDD §5 UI): New Game, Continue, language toggle.
    /// Lives in the MainMenu scene; the persistent managers come from Boot.</summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button languageButton;

        private void Start()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(() =>
                    GameManager.Instance?.StartChapter(1, "Chapter01"));
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(() => GameManager.Instance?.ContinueGame());
                continueButton.interactable = SaveSystem.Load(0) != null;
            }
            if (languageButton != null)
                languageButton.onClick.AddListener(ToggleLanguage);
            RefreshLabels();
            var loc = LocalizationManager.Instance;
            if (loc != null) loc.OnLanguageChanged += RefreshLabels;
        }

        private void OnDestroy()
        {
            var loc = LocalizationManager.Instance;
            if (loc != null) loc.OnLanguageChanged -= RefreshLabels;
        }

        private void ToggleLanguage()
        {
            var loc = LocalizationManager.Instance;
            if (loc == null) return;
            loc.SetLanguage(loc.Language == "fa" ? "en" : "fa");
        }

        private void RefreshLabels()
        {
            var loc = LocalizationManager.Instance;
            if (loc == null) return;
            SetLabel(newGameButton, loc.Get("menu.newgame"));
            SetLabel(continueButton, loc.Get("menu.continue"));
            SetLabel(languageButton, loc.Get("menu.language") + ": "
                + (loc.Language == "fa" ? "فارسی" : "English"));
        }

        private static void SetLabel(Button b, string text)
        {
            if (b == null) return;
            var label = b.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = text;
        }
    }
}
