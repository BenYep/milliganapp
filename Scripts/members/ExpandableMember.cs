using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class ExpandableMember : Member,IPointerClickHandler  {

	[HideInInspector] public bool expanded=false,
	settled=true;
	public float min_size=0,
	expanded_size=0,
	expand_rate=100;
	public GameObject more_info;

	// Use this for initialization
	void Start () {
		
	}

	public void init(){
		base.init();
	}
	
	void Update () {
		if(gameObject.name=="faculty_member"||gameObject.name=="hours_member"){
			expanded_size=min_size+more_info.GetComponent<RectTransform>().rect.height+36;
		}else{
			expanded_size=min_size+more_info.GetComponent<RectTransform>().rect.height+12;
		}

		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Began){
				RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero);
				if(hit.collider){
					if(hit.collider.name==gameObject.name){
						OnPointerClick(null);
						break;
					}
				}
			}
		}
		if(!settled){
			if(expanded){
				if(gameObject.GetComponent<LayoutElement>().preferredHeight<expanded_size){
					gameObject.GetComponent<LayoutElement>().preferredHeight+=expand_rate;
				}else{
					gameObject.GetComponent<LayoutElement>().preferredHeight=expanded_size;
					settled=true;
				}
			}else{
				if(gameObject.GetComponent<LayoutElement>().preferredHeight>min_size){
					gameObject.GetComponent<LayoutElement>().preferredHeight-=expand_rate;
				}else{
					gameObject.GetComponent<LayoutElement>().preferredHeight=min_size;
					settled=true;
				}
			}
		}
	}

	public void OnPointerClick(PointerEventData data){
		if(!expanded){
			if(Main.main.hours_expanded!=null){
				Main.main.hours_expanded.expanded=false;
				Main.main.hours_expanded.settled=false;
			}
			Main.main.hours_expanded=this;
			expanded=true;
			settled=false;
		}else{
			Main.main.hours_expanded=null;
			expanded=false;
			settled=false;
		}
	}
}
