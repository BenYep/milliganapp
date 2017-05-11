using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems; // Required when using event data

public class SubscreenContainer : MonoBehaviour, IBeginDragHandler,IEndDragHandler // required interface when using the OnEndDrag method.
{
	private bool isDragging;

	void Awake(){
		isDragging=false;
	}

	public void OnBeginDrag(PointerEventData data){
		isDragging=true;
	}
		
	public void OnEndDrag (PointerEventData data){
		if(Mathf.Abs(Main.main.current_screen_scroll_value-Main.main.subscreen_scrollbar.value)>.01f){
			if(Main.main.subscreen_scrollbar.value<Main.main.current_screen_scroll_value){
				Main.main.current_screen_scroll_value=(Main.main.current_screen_number-1)*Main.main.screen_number_unit;

			}else{
				Main.main.current_screen_scroll_value=(Main.main.current_screen_number+1)*Main.main.screen_number_unit;

			}
		}else{
			Main.main.subscreen_scrollbar.value=Main.main.current_screen_scroll_value;
		}
		isDragging=false;
	}

	void Update(){
		if(!isDragging){
			if(Mathf.Abs(Main.main.subscreen_scrollbar.value-Main.main.current_screen_scroll_value)>.005f){
				float sign=(Main.main.subscreen_scrollbar.value<Main.main.current_screen_scroll_value)?-1f:1f;
				Main.main.subscreen_scrollbar.value-=Mathf.Min(Mathf.Abs((Main.main.subscreen_scrollbar.value-Main.main.current_screen_scroll_value)/2f),.15f)*sign;
			}
		}
	}


}