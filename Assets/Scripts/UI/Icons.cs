using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    /// <summary>
    /// Stores references to icon sprites. Names are mostly the same as the asset names.
    /// </summary>
    public class Icons : MonoBehaviour
    {
        public static Icons get;

        public Sprite place;
        public Sprite addLocation;
        public Sprite myLocation;

        public Sprite brush;
        public Sprite edit;

        // Selections
        public Sprite colorLens;
        public Sprite dynamicFeed;
        public Sprite filterHdr;
        public Sprite flash;
        public Sprite localOffer;
        public Sprite style;

        // Directions
        public Sprite down;
        public Sprite left;
        public Sprite right;
        public Sprite up;
        public Sprite upDown;
        public Sprite leftRight;
        public Sprite leftRightDotted;

        // Transformations
        public Sprite openWith;
        public Sprite handFilled;
        public Sprite hand;
        public Sprite autorenew;
        public Sprite rotate90;
        public Sprite rotate;
        public Sprite screenLockRotation;
        public Sprite screenRotation;
        public Sprite star;
        public Sprite visible;
        public Sprite notVisible;
        public Sprite locked;
        public Sprite unlocked;

        public Sprite miscServices;
        public Sprite tune;
        public Sprite handyman;

        public Sprite close;
        public Sprite warn;
        public Sprite info;
        public Sprite help;

        private void Awake()
        {
            if (get)
            {
                Debug.LogWarning("Icons instance already exists.");
                enabled = false;
                return;
            }

            get = this;
        }
    }
}