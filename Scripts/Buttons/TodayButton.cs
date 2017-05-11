using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TodayButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		gameObject.name="sfp_calendar_button";
	}

	// Update is called once per frame
	void Update () {
		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Began){
				RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero);
				if(hit.collider){
					if(hit.collider.name=="sfp_calendar_button"){
						OnSFPClick();
						break;
					}
				}
			}
		}
	}

	public void OnSFPClick(){
		Main.main.current_screen_scroll_value=1f;
		Main.main.select_screen("sfp_cal");
	}
}