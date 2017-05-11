using System;
using UnityEngine;

public class FrameSmartQueue : MonoBehaviour
{
	public FrameTimer frame_timer_prefab;
	private FrameTimer _frame_timer;

	void Awake(){
		DontDestroyOnLoad(this);
		_frame_timer=(FrameTimer)Instantiate(frame_timer_prefab,Vector3.zero,Quaternion.identity);
	}

	public double getFrameTime(){
		return _frame_timer.getFrameTime();
	}
}