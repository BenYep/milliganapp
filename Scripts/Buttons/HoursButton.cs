using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HoursButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		gameObject.name="hours_button";
	}

	// Update is called once per frame
	void Update () {
		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Began){
				RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero);
				if(hit.collider){
					if(hit.collider.name=="hours_button"){
						OnHoursClick();
						break;
					}
				}
			}
		}
	}

	public void OnHoursClick(){
		Main.main.current_screen_scroll_value=.5f;
		Main.main.select_screen("hours");
	}
}