using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class FacultyMember : ExpandableMember {
	
	[HideInInspector] private string faculty_name,
	title,
	phone,
	email,
	office,
	link,
	image_url;
	private Texture2D image_texture;
	private bool initialized=false;
	private static int loading_count;
	private static readonly int LOADING_MAX=1;
	private static readonly float LOAD_CHECK_DELAY=.5f;

	public void Awake(){
		image_texture=new Texture2D(120,120);
		gameObject.name="faculty_member";
		loading_count=0;
	}

	public void Start(){
		member_data.transform.Find("text").Find("name").GetComponent<Text>().text=faculty_name;
		member_data.transform.Find("text").Find("title").GetComponent<Text>().text=title;
		more_info.GetComponent<Text>().text="phone: "+phone+"\n"+"Email: "+email+"\nOffice: "+office;
		min_size=member_data.transform.Find("text").Find("name").GetComponent<RectTransform>().rect.height+member_data.transform.Find("text").Find("title").GetComponent<RectTransform>().rect.height+13;
		gameObject.GetComponent<LayoutElement>().preferredHeight=min_size;
	}

	public void init(string name_in,string title_in,string phone_in,string email_in,string office_in,string link_in,string image_in){
		base.init();
		faculty_name=name_in;
		title=title_in;
		phone=phone_in;
		email=email_in;
		office=office_in;
		link=link_in;
		image_url=image_in;
		transform.SetParent(Main.main.faculty_data.transform);
		transform.localScale=Vector3.one;
		transform.localPosition=new Vector3(transform.localPosition.x,transform.localPosition.y,0);
		initialized=true;
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

	public IEnumerator loadMemberImage(){
		GameObject go=member_data.transform.Find("image").gameObject;

		SpriteMask.updateFor(go.GetComponent<SpriteRenderer>().transform);
		if(!initialized){ yield break;}
		if(image_url.Equals("https://www.milligan.edu/wp-content/themes/Milligan%20Theme/images/default-headshot.gif",StringComparison.InvariantCultureIgnoreCase)){
			yield break;
		}
		while(loading_count>=LOADING_MAX){
			yield return new WaitForSeconds(LOAD_CHECK_DELAY);
		}
			
		loading_count+=1;

		LoadFacultyImageJob faculty_image_job=new LoadFacultyImageJob();
		faculty_image_job.image_url=image_url;
		ThreadSmartQueue._thread_smart_queue.EnqueueItem(faculty_image_job);
		yield return StartCoroutine(faculty_image_job.WaitFor());

		string error="";

		if(faculty_image_job.failed){
			error="error";
		}

		if(error!=""){
			yield break;
		}
			
		byte[] faculty_image_data=faculty_image_job.image_data;

		if(!faculty_image_job.failed){
			image_texture.LoadImage(faculty_image_data);
			enabled=true;
			go.GetComponent<RectTransform>().sizeDelta=new Vector2(120,120);
			go.transform.localScale=new Vector3(140,140,1);
				
			go.GetComponent<SpriteRenderer>().sprite = Sprite.Create (image_texture, new Rect(0,0,image_texture.height,image_texture.height), new Vector2 (0.5f, 0.5f));
			go.GetComponent<SpriteRenderer>().sprite.name = go.GetComponent<SpriteRenderer>().name + "_sprite";
			go.GetComponent<SpriteRenderer>().material.mainTexture = image_texture as Texture;
			go.GetComponent<SpriteRenderer>().material.shader = Shader.Find ("Sprites/Default");
		}
		SpriteMask.updateFor(go.GetComponent<SpriteRenderer>().transform);

		loading_count-=1;
	}

}
	
//ACCESSORY CLASSES

public class LoadFacultyImageJob : Job
{
	public byte[] image_data;
	public string image_url;
	const int DefaultTimeout = 10 * 1000; // 2 minutes timeout
	public bool failed;

	public LoadFacultyImageJob(){
		image_data=null;
		image_url="";
		failed=false;
	}

	protected internal override void Work()
	{
		try{
			HttpWebRequest wreq = (HttpWebRequest)HttpWebRequest.Create(image_url);
			wreq.Proxy=null;
			wreq.Method = "GET";
			wreq.Timeout=DefaultTimeout;

			Stream response_stream = wreq.GetResponse().GetResponseStream();
			image_data=Main.readStream(response_stream);
			response_stream.Close();
		}catch(Exception e){
			failed=true;
		}
	}

	protected internal override void OnComplete()
	{
		
	}
}
	
//END ACCESSORY CLASSES