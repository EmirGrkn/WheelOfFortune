using UnityEngine;
using WheelOfFortune.Core;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
using WheelOfFortune.UI.Rewards;
using WheelOfFortune.UI.Wheel;
using WheelOfFortune.UI.Zone;

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
        [SerializeField] private RewardPanelView _rewardPanelView;
        [SerializeField] private ZoneBarView _zoneBarView;
        [SerializeField] private NextZoneCardView _safeZoneCard;
        [SerializeField] private NextZoneCardView _superZoneCard;

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
            _rewardPanelView.ExitClicked += HandleExitClicked;
            _session.ZoneChanged += HandleZoneChanged;
            _session.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            _wheelView.SpinClicked -= HandleSpinClicked;
            _rewardPanelView.ExitClicked -= HandleExitClicked;
            _session.ZoneChanged -= HandleZoneChanged;
            _session.StateChanged -= HandleStateChanged;
        }

        private void Start()
        {
            _rewardPanelView.Bind(_session.Inventory);
            _zoneBarView.Initialize(_progression.GetZoneType, _session.CurrentZone);
            ShowZone(_session.CurrentZone);
            HandleStateChanged(_session.State);
        }

        private void HandleSpinClicked()
        {
            if (!_session.CanSpin) return;

            SpinOutcome outcome = _session.Spin(_currentWheel.Slices);
            _wheelView.PlaySpin(outcome.SliceIndex, _session.CompleteSpin);
        }

        private void HandleExitClicked()
        {
            if (!_session.CanLeave) return;
            _session.Leave();
        }

        private void HandleZoneChanged(int zone)
        {
            ShowZone(zone);
            _zoneBarView.SetZone(zone);
        }

        private void HandleStateChanged(SessionState state)
        {
            _wheelView.SetSpinInteractable(state == SessionState.ReadyToSpin);
            _rewardPanelView.SetExitInteractable(_session.CanLeave);

            // Temporary until the bomb and collect popups exist: log and start over.
            if (state == SessionState.Busted || state == SessionState.Collected)
            {
                Debug.Log($"{state} at zone {_session.CurrentZone}. Restarting.");
                _session.Restart();
            }
        }

        private void ShowZone(int zone)
        {
            _currentWheel = _zoneConfig.GetWheel(_progression.GetZoneType(zone));
            _wheelView.SetWheel(_currentWheel, slice => _progression.ScaleRewardAmount(slice.BaseAmount, zone));
            _safeZoneCard.SetZone(_progression.GetNextZone(zone, ZoneType.Safe));
            _superZoneCard.SetZone(_progression.GetNextZone(zone, ZoneType.Super));
        }
    }
}
