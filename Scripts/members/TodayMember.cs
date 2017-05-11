using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class TodayMember : ExpandableMember {

	[HideInInspector] private TodayEvent today_event;

	public void Awake(){
		gameObject.name="today_member";
		y_view_extension=.4f;
	}

	public void Start(){
		member_data.transform.Find("text").Find("title").GetComponent<Text>().text=today_event.title;

		string formatted_start_end=today_event.start.ToString("dddd, MMMM d");
		if(!today_event.end.Equals(DateTime.MinValue)&&!(today_event.start.Year==today_event.end.Year&&today_event.start.Month==today_event.end.Month&&today_event.start.Day==today_event.end.Day)){
			formatted_start_end+=" - "+today_event.end.ToString("dddd, MMMM d");
		}
		formatted_start_end+="\n";
		if(!today_event.is_all_day){
			formatted_start_end+=today_event.start.ToString("h:mm tt");
			if(!today_event.end.Equals(DateTime.MinValue)){
				formatted_start_end+=" - "+today_event.end.ToString("h:mm tt");
			}
		}else{
			formatted_start_end+="All day";
		}

		member_data.transform.Find("text").Find("start_end").GetComponent<Text>().text=formatted_start_end;
		more_info.GetComponent<Text>().text=today_event.description;
		min_size=member_data.transform.Find("text").Find("title").GetComponent<RectTransform>().rect.height+member_data.transform.Find("text").Find("start_end").GetComponent<RectTransform>().rect.height+13;
		gameObject.GetComponent<LayoutElement>().preferredHeight=min_size;
	}

	public void init(TodayEvent t_event){
		base.init();
		today_event=t_event;
		transform.SetParent(Main.main.today_data.transform);
		transform.localScale=Vector3.one;
		transform.localPosition=new Vector3(transform.localPosition.x,transform.localPosition.y,0);
	}

	protected override void activate_member(){
		base.activate_member();
		GetComponent<RectMask2D>().enabled=true;
		GetComponent<Image>().enabled=true;
	}

	protected override void deactivate_member(){
		base.deactivate_member();
		GetComponent<RectMask2D>().enabled=false;
		GetComponent<Image>().enabled=false;
	}

}
