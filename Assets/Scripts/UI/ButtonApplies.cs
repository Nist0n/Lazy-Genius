using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class ButtonApplies : MonoBehaviour
    {
        public Button button;
        public GameObject tab;
        
        [SerializeField] private Sprite activateImage;
        [SerializeField] private Sprite defaultImage;
        [SerializeField] private Image image;

        public void ActivateTab()
        {
            if (!tab)
            {
                Debug.LogError("tab не присвоен");
                return;
            }
            tab.SetActive(true);

            if (!activateImage)
            {
                Debug.LogError("activateImage не присвоен");
                return;
            }
            image.sprite = activateImage;
        }

        public void DeactivateTab()
        {
            if (!tab)
            {
                Debug.LogError("tab не присвоен");
                return;
            }
            tab.SetActive(false);

            if (!activateImage)
            {
                Debug.LogError("activateImage не присвоен");
                return;
            }
            image.sprite = defaultImage;
        }
    }
}
