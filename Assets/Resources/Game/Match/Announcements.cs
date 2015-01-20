using System.Collections.Generic;






public class Announcements {
	public class TimingEvent {
		public float Time;
		public string Sound;
		
		public TimingEvent(float time, string sound) {
			Time = time;
			Sound = sound;
		}
	}

	bool announcedMatchStart = false;
	List<TimingEvent> times = new List<TimingEvent>();



	public void Update(float timeLeft) {
		if (!announcedMatchStart) {
			announcedMatchStart = true;
			Sfx.PlayOmni("321Fight");
		}

		if // there's pending announcements 
		(times.Count > 0 && timeLeft < times[0].Time) {
			Sfx.PlayOmni(times[0].Sound);
			times.RemoveAt(0);
		}
	}


	public void SetupTimeCountdowns() {
		announcedMatchStart = false;
		times.Clear();
		times.Add(new TimingEvent(120f, "RemainingMins2"));
		times.Add(new TimingEvent(60f, "RemainingMins1"));
		times.Add(new TimingEvent(30f, "RemainingSecs30"));
		times.Add(new TimingEvent(10f, "AlmostOver"));
	}
}
