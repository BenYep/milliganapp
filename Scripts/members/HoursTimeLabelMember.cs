using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class HoursTimeLabelMember : Member {

	private DateTime date_time;

	// Use this for initialization
	void Awake () {
		gameObject.name="hours_time_label_member";
	}

	public void init(string date_type,DateTime d_time){
		base.init();
		date_time=d_time;
		transform.SetParent(Main.main.today_data.transform);
		transform.localScale=Vector3.one;
		transform.localPosition=new Vector3(transform.localPosition.x,transform.localPosition.y,0);
		if(date_type=="month"){
			member_data.transform.Find("text").GetComponent<Text>().text=date_time.ToString("MMMM");
		}else{
			member_data.transform.Find("text").GetComponent<Text>().text=date_time.ToString("dddd, MMMM d");
		}
	}

	void Start(){
		
	}

	// Update is called once per frame
	void Update () {
	
	}
}
