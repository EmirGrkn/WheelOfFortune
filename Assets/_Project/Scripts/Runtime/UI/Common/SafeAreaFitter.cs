using UnityEngine;

namespace WheelOfFortune.UI
{
    /// <summary>
    /// Fits this RectTransform to <see cref="Screen.safeArea"/> so HUD elements stay clear of notches and camera cutouts.
    /// Full screen backgrounds and popup dims should stay outside of it.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _appliedSafeArea;
        private Vector2Int _appliedScreenSize;
        private bool _hasWarned;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            Apply();
        }

        // The safe area changes with orientation, so it is compared every frame; the check itself is cheap.
        private void Update()
        {
            if (Screen.safeArea != _appliedSafeArea || Screen.width != _appliedScreenSize.x || Screen.height != _appliedScreenSize.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            Rect safeArea = Screen.safeArea;
            var screenSize = new Vector2(Screen.width, Screen.height);

            // Remember what was seen even if it is skipped, so an invalid frame is not retried every Update.
            _appliedSafeArea = safeArea;
            _appliedScreenSize = new Vector2Int(Screen.width, Screen.height);

            // The screen can report a zero size or an invalid safe area for a frame (e.g. while the simulator starts).
            // Writing the result would give NaN anchors and hide the whole HUD, so such frames are skipped.
            // The checks are written as "!(x > 0)" so NaN, which fails every comparison, is rejected too.
            Vector2 anchorMin = safeArea.position / screenSize;
            Vector2 anchorMax = (safeArea.position + safeArea.size) / screenSize;
            if (!(screenSize.x > 0f) || !(screenSize.y > 0f) || !IsValidAnchor(anchorMin) || !IsValidAnchor(anchorMax))
            {
                // NaN never equals itself, so an invalid safe area is retried every frame; warn only once.
                if (!_hasWarned)
                {
                    Debug.LogWarning($"{name}: invalid safe area {safeArea} for screen {screenSize}, skipped.", this);
                    _hasWarned = true;
                }
                return;
            }

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }

        private static bool IsValidAnchor(Vector2 anchor) =>
            float.IsFinite(anchor.x) && float.IsFinite(anchor.y) &&
            anchor.x >= 0f && anchor.x <= 1f && anchor.y >= 0f && anchor.y <= 1f;
    }
}
