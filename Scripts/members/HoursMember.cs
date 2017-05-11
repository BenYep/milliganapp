using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HoursMember : ExpandableMember {

	void Awake(){
		gameObject.name="hours_member";
		y_view_extension=1f;
	}
	// Use this for initialization
	void Start () {
		min_size=member_data.transform.Find("Title").GetComponent<RectTransform>().rect.height;
		gameObject.GetComponent<LayoutElement>().preferredHeight=min_size;
		init();
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
