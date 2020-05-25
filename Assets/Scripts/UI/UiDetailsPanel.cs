using UnityEngine;

namespace UI
{
    public class UiDetailsPanel : MonoBehaviour
    {
        public UiProgressIcon progressIcon;

        public void PreExecute()
        {
            progressIcon.PreExecute();
        }

        public void PostExecute()
        {
            progressIcon.PostExecute();
        }
    }
}
