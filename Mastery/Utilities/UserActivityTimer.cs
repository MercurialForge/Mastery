using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mastery.Utilities
{
    public enum UserActivityState
    {
        Unknown = -1,
        Inactive = 0,
        Active = 1
    }

    [DefaultPropertyAttribute("IdleThreshold")]
    [DefaultEventAttribute("UserActiveEvent")]
    public class UserActivityTimer
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        #region Fields

        private long _idleThreshold;
        private uint _lastActivity;
        private bool _timerEnabled;
        private UserActivityState _userActiveState = UserActivityState.Unknown;
        private DateTime _lastResetTime;
        private object _tag;

        private LASTINPUTINFO lastInput;
        private Timer activityCheckerTimer;
        Stopwatch activityStopWatch = new Stopwatch();

        #endregion

        #region Events
        [Description("Occurs when the user becomes active.")]
        public event EventHandler UserActiveEvent;

        [Description("Occurs when the user becomes idle.")]
        public event EventHandler UserIdleEvent;
        #endregion

        #region Methods

        /// <summary>
        /// Constructs a new timer to track the idle/activity status of the user.
        /// Defaults to 10-second idle threshold in a disabled state
        /// </summary>
        public UserActivityTimer()
            : this(10000, false)
        {
        }

        /// <summary>
        /// Constructs a new timer to track the idle/activity status of the user.
        /// Defaults to a disabled state.
        /// </summary>
        /// <param name="idleThreshold">Number of milliseconds of inactivity until user is considered idle</param>
        public UserActivityTimer(long idleThreshold)
            : this(idleThreshold, false)
        {
        }

        /// <summary>
        /// Constructs a new timer to track the idle/activity status of the user
        /// </summary>
        /// <param name="idleThreshold">Number of milliseconds of inactivity until user is considered idle</param>
        /// <param name="active">Whether or not the timer should be created in an active state</param>
        public UserActivityTimer(long idleThreshold, bool active)
        {
            this._idleThreshold = idleThreshold;

            // Initialize system call
            this.lastInput = new LASTINPUTINFO();
            this.lastInput.cbSize = (uint)Marshal.SizeOf(this.lastInput);

            // Read initial value
            GetLastInputInfo(ref this.lastInput);
            this._lastActivity = this.lastInput.dwTime;

            // Create underlying timer
            this.activityCheckerTimer = new Timer(new TimerCallback(this.GetLastInput), null, Timeout.Infinite, 1000);
            this._lastResetTime = DateTime.Now;
            this.Enabled = active;
        }

        /// <summary>
        /// Resets the elapsed activity time to zero and continues measuring.
        /// </summary>
        public void Reset()
        {
            activityStopWatch.Reset();
            this._userActiveState = UserActivityState.Unknown;
            this._lastResetTime = DateTime.Now;

            if (this._timerEnabled)
            {
                activityStopWatch.Start();
            }
        }

        /// <summary>
        /// Disables the activity timer and associated events
        /// </summary>
        public void Disable()
        {
            // Ignore if already disabled
            if (this._timerEnabled)
            {
                this.activityCheckerTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this._timerEnabled = false;
                this._userActiveState = UserActivityState.Unknown;
                this.activityStopWatch.Stop();
            }
        }

        /// <summary>
        /// Enables the activity timer and associated events
        /// </summary>
        public void Enable()
        {
            // Ignore if already enabled
            if (!this._timerEnabled)
            {
                this.activityCheckerTimer.Change(0, 1000);
                this._timerEnabled = true;
                this.activityStopWatch.Start();
            }
        }

        /// <summary>
        /// Checks to see the last activity timestamp and compares is to the idle threshold.
        /// Will trigger appropriate events if state changes.
        /// </summary>
        /// <param name="userState">Ignored - required for ThreadStart delegate</param>
        private void GetLastInput(object userState)
        {
            GetLastInputInfo(ref this.lastInput);
            this._lastActivity = this.lastInput.dwTime;

            if ((Environment.TickCount - this.lastInput.dwTime) > this._idleThreshold)
            {
                if (this._userActiveState != UserActivityState.Inactive)
                {
                    this._userActiveState = UserActivityState.Inactive;
                    this.activityStopWatch.Stop();
                    this.RaiseUserIdleEvent();
                }
            }
            else if (this._userActiveState != UserActivityState.Active)
            {
                this._userActiveState = UserActivityState.Active;
                this.activityStopWatch.Start();
                this.RaiseUserActiveEvent();
            }
        }

        /// <summary>
        /// Performs appropriate error-checking before raising the UserActive event
        /// </summary>
        private void RaiseUserActiveEvent()
        {
            if (this.UserActiveEvent != null)
            {
                this.UserActiveEvent(this, null);
            }
        }

        /// <summary>
        /// Performs appropriate error-checking before raising the UserIdle event
        /// </summary>
        private void RaiseUserIdleEvent()
        {
            if (this.UserIdleEvent != null)
            {
                this.UserIdleEvent(this, null);
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the time the timer was created or last reset.
        /// </summary>
        [Description("The time the timer was created or last reset.")]
        public DateTime LastResetTime
        {
            get { return _lastResetTime; }
        }

        /// <summary>
        /// Gets or sets the amount of inactivity until user is considered idle, measured in milliseconds.
        /// </summary>
        [DefaultValue(10000)]
        [Description("The amount of inactivity until user is considered idle, measured in milliseconds.")]
        public long IdleThreshold
        {
            get
            {
                return this._idleThreshold;
            }
            set
            {
                this._idleThreshold = value;
            }
        }

        /// <summary>
        /// Gets the total elapsed time that the user has been active, in milliseconds.
        /// </summary>
        public long ActiveTimeMilliseconds
        {
            get
            {
                return this.activityStopWatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Gets the total elapsed time that the user has been active.
        /// </summary>
        public TimeSpan ActiveTime
        {
            get
            {
                return this.activityStopWatch.Elapsed;
            }
        }

        /// <summary>
        /// Gets or sets the current state of the timer
        /// </summary>
        [Description("The current state of the timer.")]
        public bool Enabled
        {
            get
            {
                return this._timerEnabled;
            }
            set
            {
                if (value == true) Enable();
                else Disable();
            }
        }

        /// <summary>
        /// Gets the current state of the user
        /// </summary>
        public UserActivityState UserActiveState
        {
            get
            {
                return this._userActiveState;
            }
        }
        #endregion

        #region Nested Types
        [StructLayout(LayoutKind.Sequential)]
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }
        #endregion
    }
}
