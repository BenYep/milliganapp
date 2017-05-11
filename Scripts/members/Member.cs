using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Member : BaseObject {

	public GameObject member_data;
	protected float y_view_extension=.25f;

	// Use this for initialization
	public void init () {
		GetComponent<RectTransform>().localPosition=new Vector3(transform.localPosition.x,transform.localPosition.y,0);
		InvokeRepeating("ControlActive", 0.0f, 0.2f);
	}
	
	protected void ControlActive(){
		if(Camera.current != null) {
			Vector3 pos = Camera.current.WorldToViewportPoint(transform.position);
			if(pos.z > 0 && pos.x+.15f >= 0.0f && pos.x-.15f <=1.0f && pos.y+y_view_extension >= 0.0f && pos.y-y_view_extension <=1.0f) {
				if(!enabled){
					activate_member();
				}
			}
			else{
				if(enabled){
					deactivate_member();
				}
			}
		}
	}
	protected virtual void activate_member(){
		member_data.SetActive(true);
		enabled=true;
	}
	protected virtual void deactivate_member(){
		if(this is ExpandableMember){
			ExpandableMember this_expandable=this as ExpandableMember;
			this_expandable.settled=true;
			this_expandable.expanded=false;
			this_expandable.GetComponent<LayoutElement>().preferredHeight=this_expandable.min_size;
		}
		enabled=false;
		member_data.SetActive(false);
	}
}
