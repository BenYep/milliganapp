using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelectableText : InputField {

	string perma_text;

	void Awake(){
		perma_text=text;
	}

	void OnValueChanged(){
		Debug.Log("bananas");
	}
}
