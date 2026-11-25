using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilentWard.Core
{
    /// <summary>Boot scene entry point. The persistent managers (GameManager,
    /// LocalizationManager, AudioManager) live in Boot and survive scene loads;
    /// once they are awake, hand over to the main menu.</summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private string menuScene = "MainMenu";

        private void Start()
        {
            SceneManager.LoadScene(menuScene);
        }
    }
}
