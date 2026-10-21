using System;
using UnityEngine;

namespace WheelOfFortune.UI.Zone
{
    /// <summary>How a zone type looks on the zone bar.</summary>
    [Serializable]
    public struct ZoneStyle
    {
        [Tooltip("Background of the current zone cell.")]
        public Sprite HighlightSprite;
        public Color HighlightColor;
        [Tooltip("Number color when this zone is the current one.")]
        public Color CurrentNumberColor;
        [Tooltip("Number color when this zone is still ahead.")]
        public Color UpcomingNumberColor;
    }
}
