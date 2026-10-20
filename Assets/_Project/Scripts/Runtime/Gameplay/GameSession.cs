using System;
using System.Collections.Generic;
using WheelOfFortune.Core;
using WheelOfFortune.Data;

namespace WheelOfFortune.Gameplay
{
    /// <summary>
    /// Game rules of a single run: spinning, collecting rewards, hitting the bomb and leaving.
    /// A spin has two steps so the result is known before the wheel animation starts:
    /// <see cref="Spin"/> picks the slice, <see cref="CompleteSpin"/> applies it once the animation ends.
    /// </summary>
    public sealed class GameSession
    {
        private readonly ZoneProgression _progression;
        private readonly IRandomProvider _random;
        private SpinOutcome _pendingOutcome;

        public event Action<int> ZoneChanged;
        public event Action<SessionState> StateChanged;

        public int CurrentZone { get; private set; } = ZoneProgression.FirstZone;
        public SessionState State { get; private set; } = SessionState.ReadyToSpin;
        public RewardInventory Inventory { get; } = new RewardInventory();

        public ZoneType CurrentZoneType => _progression.GetZoneType(CurrentZone);
        public bool CanSpin => State == SessionState.ReadyToSpin;
        public bool CanLeave => State == SessionState.ReadyToSpin && _progression.IsRiskFree(CurrentZone);

        public GameSession(ZoneProgression progression, IRandomProvider random)
        {
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public SpinOutcome Spin(IReadOnlyList<WheelSliceData> slices)
        {
            if (!CanSpin) throw new InvalidOperationException($"Cannot spin while in {State} state.");

            int index = WheelSliceSelector.SelectIndex(slices, _random.NextFloat());
            WheelSliceData slice = slices[index];
            int amount = slice.IsBomb ? 0 : _progression.ScaleRewardAmount(slice.BaseAmount, CurrentZone);

            _pendingOutcome = new SpinOutcome(index, slice.IsBomb, slice.Reward, amount);
            SetState(SessionState.Spinning);
            return _pendingOutcome;
        }

        public void CompleteSpin()
        {
            if (State != SessionState.Spinning) throw new InvalidOperationException("No spin in progress.");

            if (_pendingOutcome.IsBomb)
            {
                SetState(SessionState.Busted);
                return;
            }

            Inventory.Add(_pendingOutcome.Reward, _pendingOutcome.Amount);
            SetZone(CurrentZone + 1);
            SetState(SessionState.ReadyToSpin);
        }

        public void Leave()
        {
            if (!CanLeave) throw new InvalidOperationException("Leaving is only allowed before spinning in a safe or super zone.");
            SetState(SessionState.Collected);
        }

        /// <summary>Continues after a bomb, keeping the collected rewards and retrying the same zone.</summary>
        public void Revive()
        {
            if (State != SessionState.Busted) throw new InvalidOperationException("Revive is only possible after hitting a bomb.");
            SetState(SessionState.ReadyToSpin);
        }

        public void Restart()
        {
            Inventory.Clear();
            SetZone(ZoneProgression.FirstZone);
            SetState(SessionState.ReadyToSpin);
        }

        private void SetZone(int zone)
        {
            CurrentZone = zone;
            ZoneChanged?.Invoke(zone);
        }

        private void SetState(SessionState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}
