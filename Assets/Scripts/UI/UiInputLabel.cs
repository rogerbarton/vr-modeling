using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [System.Serializable]
    public class UiInputLabelData
    {
        public bool isActive;
        public Sprite icon;
        public string text;
    }
    
    public class UiInputLabel : MonoBehaviour
    {
        public Image background;
        public Image icon;
        public TMP_Text text;

        public void SetData(UiInputLabelData data)
        {
            gameObject.SetActive(data.isActive);
            if (!data.isActive) return;
            
            icon.gameObject.SetActive(data.icon);
            icon.sprite = data.icon;
            text.text = data.text;
                
            // We need to refresh the layout explicitly
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) transform);
        }
    }
}