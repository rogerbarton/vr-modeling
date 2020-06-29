using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hints
{
    /// <summary>
    /// A label for a physical input button/axis to give a hint to what it does.
    /// </summary>
    public class UiInputLabel : MonoBehaviour
    {
        public Image background;
        public Image icon;
        public TMP_Text text;

        private UiInputLabelData _data;
        private Color _defaultColor = Color.white;

        public void Initialize()
        {
            _defaultColor = background.color;
        }

        public void SetData(UiInputLabelData data, bool overwriteData = true)
        {
            if(!data.isOverride) return;
            if(overwriteData) _data = data;
            
            gameObject.SetActive(data.isActive);
            if (!data.isActive) return;
            
            icon.gameObject.SetActive(data.icon);
            icon.sprite = data.icon;
            text.text = data.text;
                
            background.color = _defaultColor;

            // We need to refresh the layout explicitly
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) transform);
        }

        #region Directly Modify & Override

        public void SetText(string data)
        {
            text.text = data;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) transform);
        }

        public void SetColor(Color value)
        {
            background.color = value;
        }

        public void SetIcon(Sprite sprite)
        {
            var prev = icon.gameObject.activeSelf;
            icon.gameObject.SetActive(sprite);
            icon.sprite = sprite;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) transform);
        }

        /// <summary>
        /// Resets the label to the last written data, set when <see cref="SetData"/> overwriteData = true.
        /// </summary>
        public void ResetToData()
        {
            SetData(_data);
            SetColor(_defaultColor);
        }

        #endregion
    }
}