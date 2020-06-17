using System;
using Libigl;
using UnityEngine;

namespace XrInput
{
    public class XrBrush : MonoBehaviour
    {
        public Transform center;
    
        // min and max radius
        public static Vector2 RadiusRange = new Vector2(0.025f, 1f);
        public const float ResizeSpeed = 0.5f;
        
        private bool _isRight;

        public void SetRadius(float value)
        {
            transform.localScale = new Vector3(value, value, value);
        }

        public void Initialize(bool isRight)
        {
            _isRight = isRight;
            OnActiveToolChanged();
        }

        public void OnActiveToolChanged()
        {
            switch (InputManager.State.ActiveTool)
            {
                case ToolType.Transform:
                    gameObject.SetActive(false);
                    break;
                case ToolType.Select:
                    gameObject.SetActive(_isRight);
                    break;
            }
        }
    }
}
