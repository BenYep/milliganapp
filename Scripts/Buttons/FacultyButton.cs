using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FacultyButton : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		gameObject.name="faculty_button";
	}
	
	// Update is called once per frame
	void Update () {
		///GUI UPDATE

		/// //END GUI UPDATE

		foreach (Touch touch in Input.touches) {
			if (touch.phase == TouchPhase.Began){
				RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero);
				if(hit.collider){
					if(hit.collider.name=="faculty_button"){
						OnFacultyClick();
						break;
					}
				}
			}
		}
	}

	public void OnFacultyClick(){
		Main.main.current_screen_scroll_value=0;
		Main.main.select_screen("faculty");
	}
}