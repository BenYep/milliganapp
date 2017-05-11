using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class FrameTimer : MonoBehaviour
{
	public Stopwatch _frame_timer;
	private double _time_on_enter_frame; //milliseconds at enterFrame

	void Awake(){
		_frame_timer=new Stopwatch();
		_frame_timer.Start();
		_time_on_enter_frame=0;
	}

	void Start(){
		transform.parent=Main.main.script_graphics_root.transform;
	}

	void Update(){
		_time_on_enter_frame=_frame_timer.Elapsed.TotalMilliseconds;
	}

	public double getTimeOnEnterFrame(){
		return _time_on_enter_frame;
	}
	public double getFrameTime(){
		return _frame_timer.Elapsed.TotalMilliseconds-_time_on_enter_frame;
	}
}