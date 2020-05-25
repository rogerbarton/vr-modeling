using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiSelection : MonoBehaviour
    {
        public TMP_Text text;
        public Button visibleBtn;
        public Button editBtn;
        public Button clearBtn;

        // Switching Icons
        [SerializeField] private Image visibleImage = null;

        [SerializeField] private Sprite visibleSprite = null;
        [SerializeField] private Sprite visibleOffSprite = null;
        
        [SerializeField] private Image editImage = null;

        [SerializeField] private Sprite editSprite = null;
        [SerializeField] private Sprite activeSprite = null;
        
        public void ToggleVisibleSprite(bool isVisible)
        {
            visibleImage.sprite = isVisible ? visibleSprite : visibleOffSprite;
        }
        
        public void ToggleEditSprite(bool isActive)
        {
            editImage.sprite = isActive ? activeSprite : editSprite;
        }
    }
}
