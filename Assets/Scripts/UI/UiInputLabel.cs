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
            icon.sprite = data.icon;
            text.text = data.text;
        }
    }
}