using UnityEngine;
using WheelOfFortune.Core;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
using WheelOfFortune.UI.Wheel;

namespace WheelOfFortune.Game
{
    /// <summary>
    /// Composition root of the game scene: creates the session and connects it to the views.
    /// Holds no game rules itself; those live in <see cref="GameSession"/>.
    /// </summary>
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private ZoneConfig _zoneConfig;
        [SerializeField] private WheelView _wheelView;

        private ZoneProgression _progression;
        private GameSession _session;
        private WheelData _currentWheel;

        private void Awake()
        {
            _progression = _zoneConfig.CreateProgression();
            _session = new GameSession(_progression, new SystemRandomProvider());
        }

        private void OnEnable()
        {
            _wheelView.SpinClicked += HandleSpinClicked;
            _session.ZoneChanged += HandleZoneChanged;
            _session.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            _wheelView.SpinClicked -= HandleSpinClicked;
            _session.ZoneChanged -= HandleZoneChanged;
            _session.StateChanged -= HandleStateChanged;
        }

        private void Start()
        {
            ShowZone(_session.CurrentZone);
            HandleStateChanged(_session.State);
        }

        private void HandleSpinClicked()
        {
            if (!_session.CanSpin) return;

            SpinOutcome outcome = _session.Spin(_currentWheel.Slices);
            _wheelView.PlaySpin(outcome.SliceIndex, _session.CompleteSpin);
        }

        private void HandleZoneChanged(int zone) => ShowZone(zone);

        private void HandleStateChanged(SessionState state)
        {
            _wheelView.SetSpinInteractable(state == SessionState.ReadyToSpin);

            // Temporary until the bomb popup exists: log and start over.
            if (state == SessionState.Busted)
            {
                Debug.Log($"Bomb at zone {_session.CurrentZone}. Restarting.");
                _session.Restart();
            }
        }

        private void ShowZone(int zone)
        {
            _currentWheel = _zoneConfig.GetWheel(_progression.GetZoneType(zone));
            _wheelView.SetWheel(_currentWheel, slice => _progression.ScaleRewardAmount(slice.BaseAmount, zone));
        }
    }
}
