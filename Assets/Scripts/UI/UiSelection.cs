using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiSelection : MonoBehaviour
    {
        public TMP_Text text;
        public Button editBtn;
        public Button clearBtn;

        [SerializeField] private Image editImage;

        [SerializeField] private Sprite editSprite;
        [SerializeField] private Sprite activeSprite;


        public void ToggleEditSprite(bool isActive)
        {
            editImage.sprite = isActive ? activeSprite : editSprite;
        }
    }
}
