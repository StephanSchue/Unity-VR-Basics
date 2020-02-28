using System;
using UnityEngine;

namespace Utils
{
	public class Timer
	{
		public Timer(float time, bool started, Action onTriggered = null)
		{
			this.time = time;
			OnTriggered = onTriggered;
			finished = !started;

			curTime = started ? 0.0f : time;
		}

		public Timer(float time, Action onTriggered = null)
			: this(time, true, onTriggered)
		{ }

		public Timer(Action onTriggered = null)
			: this(0.0f, false, onTriggered)
		{ }

		private float curTime = 0.0f;
		private float time = 0.0f;

		public event Action OnTriggered;

		public bool finished { get; private set; }

		public float percentage
		{
			get { return Math.Abs(time) < 0.001f ? 0.0f : System.Math.Min(curTime / time, 1.0f); }
			set
			{
				finished = percentage >= 1.0f;
				curTime = time * Mathf.Clamp01(value);
			}
		}

		public bool Update(float dt)
		{
			if (finished)
				return false;

			curTime += dt;

			if (curTime > time)
			{
				finished = true;

                if(OnTriggered != null)
                    OnTriggered.Invoke();
				return true;
			}
			return false;
		}

		public bool Update()
		{
			return Update(Time.deltaTime);
		}

		public void Reset(float time, Action onTriggered = null)
		{
			curTime = 0.0f;
			this.time = time;
			finished = (time <= 0.0f);
			OnTriggered = onTriggered;
		}

		public void Reset(Action onTriggered = null)
		{
			Reset(time, onTriggered);
		}
	}
}