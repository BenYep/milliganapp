using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using HtmlAgilityPack;
using iCal_sync.ical_NET.Model;

public class Main : MonoBehaviour {
	//MAIN
	[HideInInspector] public static Main main;
	public static readonly double FRAME_MILLISECONDS = 1000/(60d);//1000 / frames per second 
	public FrameSmartQueue _frame_smart_queue;
	public GameObject script_graphics_root;
	public Camera main_camera;
	private float window_aspect;
	public float target_aspect;
	public Text debug_box;
	[HideInInspector] public string debug_text="";
	public GameObject main_screen;
	public GameObject subscreen_container;
	public GameObject[] subscreens;
	public GameObject[] headerscreen_buttons;
	[HideInInspector] public float current_screen_scroll_value;
	[HideInInspector] public float screen_number_unit;
	[HideInInspector] public int current_screen_number;
	public GameObject screen_header;
	public Scrollbar subscreen_scrollbar;
	public GameObject screen_scroller;
	public GameObject faculty_button, hours_button, today_button;
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private Canvas canvas;
	[SerializeField] private int referenceDPI;
	[SerializeField] private float referencePixelDrag;
	//END MAIN
	//FACULTY
	public static readonly string FACULTY_URL="https://www.milligan.edu/wp-content/themes/milligan%20theme/loophandler-directory.php?numPosts=1000&pageNumber=1&category=&keyword=";
	public GameObject faculty_screen;
	public GameObject faculty_root;
	public GameObject faculty_data;
	[HideInInspector] public ExpandableMember faculty_expanded;
	public FacultyMember faculty_member_prefab;
	public Scrollbar faculty_data_scrollbar;
	private LoadFacultyJob faculty_job;
	private bool faculty_screen_loaded=false;
	[HideInInspector] public SpriteRenderer faculty_screen_loader;
	//END FACULTY
	//HOURS
	public GameObject hours_data;
	[HideInInspector] public ExpandableMember hours_expanded;
	//END HOURS
	//MILLIGAN TODAY
	public static readonly string TODAY_URL="https://www.milligan.edu/?plugin=all-in-one-event-calendar&controller=ai1ec_exporter_controller&action=export_events";
	private LoadTodayCalendarJob today_job;
	[HideInInspector] public ExpandableMember today_expanded;
	public TodayMember today_member_prefab;
	public HoursTimeLabelMember hours_time_label_member_prefab;
	public GameObject today_data;
	private bool today_screen_loaded=false;
	[HideInInspector] public SpriteRenderer today_screen_loader;
	//verify: http://severinghaus.org/projects/icv/?url=http%3A%2F%2Fwww.milligan.edu%2F%3Fplugin%3Dall-in-one-event-calendar%26controller%3Dai1ec_exporter_controller%26action%3Dexport_events
	//END MILLIGAN TODAY
	//TEXTURES
	public Texture2D[] load_animation_textures;
	//END TEXTURES

	void Awake () {
		//GLOBAL INITIALIZATION
		DontDestroyOnLoad(this);
		Main.main=this;
		window_aspect=(float)Screen.width/(float)Screen.height;
		target_aspect=9f/16f;
		current_screen_scroll_value=.5f;
		ServicePointManager.DefaultConnectionLimit = 1;
		ServicePointManager.Expect100Continue = false;
		ServicePointManager.MaxServicePointIdleTime = 500;
		//END GLOBAL INITIALIZATION
		//FACULTY
		//END FACULTY
		//HOURS
		//HOURS_URL="https://www.milligan.edu/student-life/campus-hours/";
		hours_data.GetComponent<RectTransform>().anchoredPosition=Vector2.zero;
		//END HOURS
		//MILLIGAN TODAY
		//END MILLIGAN TODAY
	}

	void Start(){
		//debug_box.gameObject.SetActive(true);
		ThreadSmartQueue._thread_smart_queue=new ThreadSmartQueue(Math.Max(Environment.ProcessorCount-1,1));
		StartCoroutine(loadTodayData());
		StartCoroutine(loadFacultyData());
		UpdatePixelDrag(Screen.dpi);
		//GLOBAL GUI INITIALIZATION
		main_camera.GetComponent<Camera>().orthographicSize=1f;
		initialize_camera();
		//main_camera.GetComponent<Camera>().transform.localScale=new Vector3(Screen.height,Screen.height,1);

		float main_camera_height;
		float main_camera_width;

		main_camera_width=main_screen.GetComponent<RectTransform>().rect.width*main_camera.rect.width;
		main_camera_height=main_screen.GetComponent<RectTransform>().rect.height*main_camera.rect.height;

		screen_number_unit=1f/(subscreens.Length-1f);
		current_screen_number=Mathf.RoundToInt(current_screen_scroll_value/screen_number_unit);

		subscreen_container.GetComponent<LayoutElement>().preferredWidth=main_camera_width;

		foreach(GameObject subscreen in subscreens){
			subscreen.GetComponent<LayoutElement>().preferredWidth=main_camera_width;
			subscreen.GetComponent<LayoutElement>().minWidth=main_camera_width;
		}

		subscreen_scrollbar.value=.5f;
		//END GLOBAL GUI INITIALIZATION
		//FACULTY
		//END FACULTY
	}

	public void UpdatePixelDrag(float screenDpi)
	{
		if (eventSystem == null)
		{
			UnityEngine.Debug.LogWarning("Trying to set pixel drag for adapting to screen dpi, " +
				"but there is no event system assigned to the script", this);
		}
		eventSystem.pixelDragThreshold = Mathf.RoundToInt(screenDpi/ referenceDPI*referencePixelDrag);
	}

	void OnDestroy(){
		//_thread_smart_queue.Shutdown(false);
	}

	void Update () {
		//GUI UPDATES
		if(debug_box.IsActive()){
			debug_box.text=debug_text;
		}
		current_screen_number=Mathf.RoundToInt(current_screen_scroll_value/screen_number_unit);
		for(int i=0;i<headerscreen_buttons.Length;i++){
			Vector3 new_scale=new Vector3(0,1,0);
			if(i==current_screen_number){
				new_scale.x=1;
			}
			headerscreen_buttons[i].transform.Find("underline").transform.localScale=new_scale;
		}
		//END GUI UPDATES
	}

	//Global Helpers

	void initialize_camera(){

		// current viewport height should be scaled by this amount
		float scaleheight = window_aspect / target_aspect;

		// obtain camera component so we can modify its viewport
		Camera camera = main_camera;

		Rect rect;
		// if scaled height is less than current height, add letterbox
		if (scaleheight < 1.0f)
		{
			rect = camera.rect;

			rect.width = 1.0f;
			rect.height = scaleheight;
			rect.x = 0;
			rect.y = (1.0f - scaleheight) / 2.0f;

			camera.rect = rect;
		}
		else // add pillarbox
		{
			float scalewidth = 1.0f / scaleheight;

			rect = camera.rect;

			rect.width = scalewidth;
			rect.height = 1.0f;
			rect.x = (1.0f - scalewidth) / 2.0f;
			rect.y = 0;
			
			camera.rect = rect;
		}

	}
		
	public static byte[] readStream(Stream input)
	{
		byte[] buffer = new byte[8*1024];
		using (MemoryStream ms = new MemoryStream())
		{
			int read;
			while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				ms.Write(buffer, 0, read);
			}
			return ms.ToArray();
		}
	}

	public void select_screen(string screen){
		switch (screen){
		case "faculty":
			faculty_button.transform.Find("Text").GetComponent<Text>().color=new Color(0x73,0x7B,0x99);
			break;
		case "hours":
			hours_button.transform.Find("Text").GetComponent<Text>().color=new Color(0x73,0x7B,0x99);
			break;
		case "today":
			today_button.transform.Find("Text").GetComponent<Text>().color=new Color(0x73,0x7B,0x99);
			break;

		}
	}
	private void reset_screen_buttons(){//0x737B99FF
		faculty_button.transform.Find("Text").GetComponent<Text>().color=new Color(0xFF,0xFF,0xFF);
		hours_button.transform.Find("Text").GetComponent<Text>().color=new Color(0xFF,0xFF,0xFF);
		today_button.transform.Find("Text").GetComponent<Text>().color=new Color(0xFF,0xFF,0xFF);
	}
	//END GLOBAL HELPERS

	//FACULTY HELPERS
	private IEnumerator loadFacultyData(){
		StartCoroutine(ShowFacultyLoadAnimation());
		faculty_data.SetActive(false);
		faculty_job=new LoadFacultyJob();
		ThreadSmartQueue._thread_smart_queue.EnqueueItem(faculty_job);
		yield return StartCoroutine(faculty_job.WaitFor());
		List<FacultyMember> faculty_members=new List<FacultyMember>();
		foreach(Dictionary<String,String> faculty_member_data in faculty_job.faculty_data){
			FacultyMember faculty_member=(FacultyMember)Instantiate(Main.main.faculty_member_prefab,Vector3.zero,Quaternion.identity);
			faculty_member.init(faculty_member_data["name"],faculty_member_data["title"],faculty_member_data["phone"],faculty_member_data["email"],faculty_member_data["office"],faculty_member_data["link"],faculty_member_data["image"]);
			faculty_members.Add(faculty_member);
			if(_frame_smart_queue.getFrameTime()>=Main.FRAME_MILLISECONDS){
				yield return null;
				yield return null;
			}
		}
		faculty_screen_loader.gameObject.SetActive(false);
		faculty_screen_loaded=true;
		if(_frame_smart_queue.getFrameTime()>=Main.FRAME_MILLISECONDS){
			yield return null;
		}
		faculty_data.SetActive(true);

		if(_frame_smart_queue.getFrameTime()>=Main.FRAME_MILLISECONDS){
			yield return null;
		}
		faculty_data.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,0);
		Vector2 faculty_mask_size=Main.main.faculty_root.GetComponent<SpriteMask>().size;
		faculty_mask_size.y=Main.main.faculty_root.GetComponent<RectTransform>().rect.height;
		faculty_mask_size.x=Main.main.faculty_root.GetComponent<RectTransform>().rect.width;
		faculty_root.GetComponent<SpriteMask>().size=faculty_mask_size;

		foreach(FacultyMember faculty_member in faculty_members){
			StartCoroutine(faculty_member.loadMemberImage());
			yield return null;
		}
	}

	private IEnumerator ShowFacultyLoadAnimation(){
		while(!faculty_screen_loaded){
			for(int i=0;i<load_animation_textures.Length;i++){
				faculty_screen_loader.sprite = Sprite.Create (load_animation_textures[i], new Rect(0,0,load_animation_textures[i].height,load_animation_textures[i].height), new Vector2 (0.5f, 0.5f));
				faculty_screen_loader.sprite.name = faculty_screen_loader.name + "_sprite";
				faculty_screen_loader.material.mainTexture = load_animation_textures[i] as Texture;
				faculty_screen_loader.material.shader = Shader.Find ("Sprites/Default");
				SpriteMask.updateFor(faculty_screen_loader.transform);
				yield return null;
				yield return null;
			}
		}
	}
	//END FACULTY HELPERS

	//HOURS HELPERS

	//END HOURS HELPERS
	//TODAY HELPERS
	private IEnumerator loadTodayData(){
		StartCoroutine(ShowTodayLoadAnimation());
		today_data.SetActive(false);
		today_job=new LoadTodayCalendarJob();
		ThreadSmartQueue._thread_smart_queue.EnqueueItem(today_job);
		yield return StartCoroutine(today_job.WaitFor());
		today_screen_loader.gameObject.SetActive(false);
		today_screen_loaded=true;
		DateTime current_month=DateTime.MinValue;
		DateTime next_month=DateTime.MinValue;
		if(today_job.today_events.Count>0){
			current_month=new DateTime(today_job.today_events[0].start.Year,today_job.today_events[0].start.Month,1);
			next_month=current_month.AddMonths(1);
		}

		HoursTimeLabelMember first_month_label=(HoursTimeLabelMember)Instantiate(Main.main.hours_time_label_member_prefab,Vector3.zero,Quaternion.identity);
		first_month_label.init("month",current_month);

		foreach(TodayEvent today_event in today_job.today_events){
			if(DateTime.Compare(today_event.start,next_month)>=0){
				HoursTimeLabelMember month_label=(HoursTimeLabelMember)Instantiate(Main.main.hours_time_label_member_prefab,Vector3.zero,Quaternion.identity);
				month_label.init("month",next_month);
				next_month=next_month.AddMonths(1);
			}

			if(_frame_smart_queue.getFrameTime()>=Main.FRAME_MILLISECONDS){
				yield return null;
			}

			TodayMember today_member=(TodayMember)Instantiate(Main.main.today_member_prefab,Vector3.zero,Quaternion.identity);
			today_member.init(today_event);

			if(_frame_smart_queue.getFrameTime()>=Main.FRAME_MILLISECONDS){
				yield return null;
			}
		}

		today_data.SetActive(true);

	}
	private IEnumerator ShowTodayLoadAnimation(){
		while(!today_screen_loaded){
			for(int i=0;i<load_animation_textures.Length;i++){
				today_screen_loader.sprite = Sprite.Create (load_animation_textures[i], new Rect(0,0,load_animation_textures[i].height,load_animation_textures[i].height), new Vector2 (0.5f, 0.5f));
				today_screen_loader.sprite.name = today_screen_loader.name + "_sprite";
				today_screen_loader.material.mainTexture = load_animation_textures[i] as Texture;
				today_screen_loader.material.shader = Shader.Find ("Sprites/Default");
				SpriteMask.updateFor(today_screen_loader.transform);
				yield return null;
				yield return null;
			}
		}
	}
	//END TODAY HELPERS
}
//ACCESSORY CLASSES
public class TodayEvent{

	private DateTime _start;
	public DateTime start{
		get{
			return _start;
		}
	}
	private DateTime _end;
	public DateTime end{
		get{
			return _end;
		}
	}
	private string _title;
	public string title{
		get{
			return _title;
		}
	}
	private string _description;
	public string description{
		get{
			return _description;
		}
	}
	private string _location;
	public string location{
		get{
			return _location;
		}
	}
	private bool _is_all_day;
	public bool is_all_day{
		get{
			return _is_all_day;
		}
	}
	private List<string> _tags;
	public List<string> tags{
		get{
			return _tags;
		}
	}

	public TodayEvent(DateTime start, DateTime end,string title,string description,string location,bool is_all_day,List<string> tags){
		_start=start;
		_end=end;
		_title=title;
		_description=description;
		_location=location;
		_is_all_day=is_all_day;
		_tags=tags;
	}
	public int CompareTo(TodayEvent other)
	{
		// A null value means that this object is greater.
		if (other == null)
			return 1;

		else
			return DateTime.Compare(_start,other._start);
	}

}

public class LoadTodayCalendarJob : Job
{
	public List<TodayEvent> today_events;
	private static readonly int WEEKS_LOADED=8;
	private static readonly int MONTHS_LOADED=2;
	private static readonly int YEARS_LOADED=1;
	public static Dictionary<string,int> WEEK_DAYS,WEEK_DAYS_SHORT;

	public LoadTodayCalendarJob(){
		today_events=new List<TodayEvent>();
		WEEK_DAYS=new Dictionary<string,int>();
		WEEK_DAYS.Add("Sunday",0);
		WEEK_DAYS.Add("Monday",1);
		WEEK_DAYS.Add("Tuesday",2);
		WEEK_DAYS.Add("Wednesday",3);
		WEEK_DAYS.Add("Thursday",4);
		WEEK_DAYS.Add("Friday",5);
		WEEK_DAYS.Add("Saturday",6);

		WEEK_DAYS_SHORT=new Dictionary<string,int>();
		WEEK_DAYS_SHORT.Add("SU",0);
		WEEK_DAYS_SHORT.Add("MO",1);
		WEEK_DAYS_SHORT.Add("TU",2);
		WEEK_DAYS_SHORT.Add("WE",3);
		WEEK_DAYS_SHORT.Add("TH",4);
		WEEK_DAYS_SHORT.Add("FR",5);
		WEEK_DAYS_SHORT.Add("SA",6);
	}

	protected internal override void Work()
	{
		HttpWebRequest today_cal_request=(HttpWebRequest)HttpWebRequest.Create(Main.TODAY_URL);
		today_cal_request.AllowAutoRedirect=true;
		today_cal_request.AllowWriteStreamBuffering=true;
		today_cal_request.KeepAlive=false;
		today_cal_request.SendChunked=false;
		today_cal_request.ContentType="text/calendar";
		today_cal_request.Method="GET";
		today_cal_request.UseDefaultCredentials=true;
		today_cal_request.Proxy=null;
		HttpWebResponse today_cal_response=(HttpWebResponse)today_cal_request.GetResponse();
		Stream today_cal_response_stream=today_cal_response.GetResponseStream();
		byte[] today_cal_response_bytes=Main.readStream(today_cal_response_stream);
		string today_cal_response_str=System.Text.Encoding.UTF8.GetString(today_cal_response_bytes);
		vCalendar today_cal=new vCalendar(today_cal_response_str);
		CultureInfo en_us=new CultureInfo("en-US",true);
		char[] comma_delimiter={','};
		char[] semi_colon_delimiter={';'};
		char[] equal_sign_delimiter={'='};
		foreach(vEvent cal_event in today_cal.vEvents){
			//START EVENT CODE
			DateTime cal_event_start=DateTime.MinValue;
			DateTime cal_event_end=DateTime.MinValue;
			string cal_event_summary="";
			string cal_event_description="";
			string cal_event_location="";
			bool cal_event_is_all_day=false;
			List<string> cal_event_repeat_days=new List<string>();
			DateTime cal_event_repeat_end=DateTime.MinValue;
			string cal_event_repeat_frequency="";
			List<string> cal_event_tags=new List<string>();
			bool cal_event_start_long=true;
			try{
				cal_event_start=DateTime.ParseExact(cal_event.GetContentLine("DTSTART").Value,"yyyyMMddTHHmmss",en_us);
			}catch(Exception e){
				cal_event_start_long=false;
				cal_event_is_all_day=true;
			}
			if(!cal_event_start_long){
				try{
					cal_event_start=DateTime.ParseExact(cal_event.GetContentLine("DTSTART").Value,"yyyyMMdd",en_us);
				}catch(Exception e){

				}
			}

			bool cal_event_end_long=true;
			try{
				cal_event_end=DateTime.ParseExact(cal_event.GetContentLine("DTEND").Value,"yyyyMMddTHHmmss",en_us);
			}catch(Exception e){
				cal_event_end_long=false;
			}
			if(!cal_event_end_long){
				try{
					cal_event_end=DateTime.ParseExact(cal_event.GetContentLine("DTEND").Value,"yyyyMMdd",en_us);
				}catch(Exception e){

				}
			}

			try{
				cal_event_summary=cal_event.GetContentLine("SUMMARY").Value;
			}catch(Exception e){

			}

			try{
				cal_event_description=cal_event.GetContentLine("DESCRIPTION").Value;
			}catch(Exception e){

			}

			try{
				cal_event_location=cal_event.GetContentLine("LOCATION").Value;
			}catch(Exception e){

			}

			try{
				cal_event_tags=new List<string>(cal_event.GetContentLine("X-TAGS").Value.Split(comma_delimiter));
			}catch(Exception e){

			}

			cal_event_summary=normalize_ical_string(cal_event_summary);

			cal_event_description=normalize_ical_string(cal_event_description);

			cal_event_location=normalize_ical_string(cal_event_location);

			for(int i=0;i<cal_event_tags.Count;i++){
				cal_event_tags[i]=normalize_ical_string(cal_event_tags[i]);
			}

			cal_event_description=HtmlToText.ConvertHtml(cal_event_description);
			cal_event_description=Regex.Replace(cal_event_description,"<[^>]*>","");

			today_events.Add(new TodayEvent(cal_event_start,cal_event_end,cal_event_summary,cal_event_description,cal_event_location,cal_event_is_all_day,cal_event_tags));

			//END EVENT CODE
			//START REPEAT CODE

			try{
				List<string> repeat_parts=new List<string>(cal_event.GetContentLine("RRULE").Value.Split(semi_colon_delimiter));
				foreach(string repeat_part in repeat_parts){
					if(repeat_part.Contains("BYDAY")){
						List<string> byday_part=new List<string>(repeat_part.Split(equal_sign_delimiter));
						cal_event_repeat_days=new List<string>(byday_part[1].Split(comma_delimiter));
					}else if(repeat_part.Contains("FREQ")){
						List<string> repeat_frequency_part=new List<string>(repeat_part.Split(equal_sign_delimiter));
						cal_event_repeat_frequency=repeat_frequency_part[1];
					}else if(repeat_part.Contains("UNTIL")){
						List<string> repeat_end_part=new List<string>(repeat_part.Split(equal_sign_delimiter));
						cal_event_repeat_end=DateTime.ParseExact(repeat_end_part[1],"yyyyMMddTHHmmss",en_us);
					}
				}

			}catch(Exception e){

			}

			if(cal_event_repeat_end==DateTime.MinValue){
				try{
					List<string> repeat_parts=new List<string>(cal_event.GetContentLine("RRULE").Value.Split(semi_colon_delimiter));
					foreach(string repeat_part in repeat_parts){
						if(repeat_part.Contains("UNTIL")){
							List<string> repeat_end_part=new List<string>(repeat_part.Split(equal_sign_delimiter));
							cal_event_repeat_end=DateTime.ParseExact(repeat_end_part[1],"yyyyMMdd",en_us);
						}
					}
				}
				catch(Exception e){

				}
			}

			foreach(string repeat_day in cal_event_repeat_days){
				if(cal_event_repeat_frequency=="WEEKLY"){
					for(int i=0;i<WEEKS_LOADED;i++){
						int offset=WEEK_DAYS_SHORT[repeat_day]-WEEK_DAYS[cal_event_start.DayOfWeek.ToString()];
						if(i==0){
							if(offset<=0){continue;}
						}
							
						DateTime adjusted_start=cal_event_start.AddDays(i*7+offset);

							
						if(DateTime.Compare(adjusted_start,cal_event_repeat_end)>0){
							continue;
						}
							
						DateTime adjusted_end=(cal_event_end!=DateTime.MinValue)?cal_event_end.AddDays(i*7+offset):DateTime.MinValue;
						today_events.Add(new TodayEvent(adjusted_start,adjusted_end,cal_event_summary,cal_event_description,cal_event_location,cal_event_is_all_day,cal_event_tags));
					}
				}else if(cal_event_repeat_frequency=="MONTHLY"){
					for(int i=1;i<MONTHS_LOADED;i++){
						DateTime adjusted_start=cal_event_start.AddMonths(i);

						if(DateTime.Compare(adjusted_start,cal_event_repeat_end)>0){
							continue;
						}

						DateTime adjusted_end=(cal_event_end!=DateTime.MinValue)?cal_event_end.AddMonths(i):DateTime.MinValue;
						today_events.Add(new TodayEvent(adjusted_start,adjusted_end,cal_event_summary,cal_event_description,cal_event_location,cal_event_is_all_day,cal_event_tags));
					}
				}else if(cal_event_repeat_frequency=="YEARLY"){
					for(int i=1;i<YEARS_LOADED;i++){
						DateTime adjusted_start=cal_event_start.AddYears(i);

						if(DateTime.Compare(adjusted_start,cal_event_repeat_end)>0){
							continue;
						}

						DateTime adjusted_end=(cal_event_end!=DateTime.MinValue)?cal_event_end.AddYears(i):DateTime.MinValue;
						today_events.Add(new TodayEvent(adjusted_start,adjusted_end,cal_event_summary,cal_event_description,cal_event_location,cal_event_is_all_day,cal_event_tags));
					}
				}
			}
			//END REPEAT CODE
		}
		today_events.Sort((a,b) => a.CompareTo(b));
	}

	private string normalize_ical_string(string input){
		input=input.Replace("\\\\","\\");
		input=input.Replace("\\n","");
		input=input.Replace("\\,",",");
		input=input.Replace("\\;",";");
		return input;
	}

	protected internal override void OnComplete()
	{

	}
}

public class LoadFacultyJob : Job
{
	public List<Dictionary<String,String>> faculty_data;

	public LoadFacultyJob(){
		faculty_data=new List<Dictionary<String,String>>();
	}

	protected internal override void Work()
	{
		WebRequest faculty_request=WebRequest.Create(Main.FACULTY_URL);
		faculty_request.ContentType="application/x-www-form-urlencoded";
		faculty_request.Method = "GET";
		Stream faculty_response_stream=faculty_request.GetResponse().GetResponseStream();
		byte[] faculty_request_data=Main.readStream(faculty_response_stream);
		faculty_response_stream.Close();
		string response_str=Encoding.UTF8.GetString(faculty_request_data);
		parseFaculty(response_str);
	}
	protected internal override void OnComplete()
	{
		
	}

	private void parseFaculty(string input){
		HtmlDocument doc = new HtmlDocument();
		doc.LoadHtml(input);
		foreach(HtmlNode article in doc.DocumentNode.SelectNodes("//article")){
			string name="",title="",phone="",email="",office="";
			string link=article.ChildNodes[1].ChildNodes[3].GetAttributeValue("href","false");
			string image=article.ChildNodes[1].ChildNodes[3].ChildNodes[0].GetAttributeValue("src","false");
			HtmlNode contact=article.ChildNodes[3].ChildNodes[0];
			if(contact.ChildNodes.Count>=1){
				if(contact.ChildNodes[0].InnerText.Length>=7){
					if(contact.ChildNodes[0].InnerText.Substring(0,5)=="Phone"){
						phone=contact.ChildNodes[0].InnerText.Substring(7);
					}else if(contact.ChildNodes[0].InnerText.Substring(0,5)=="Email"){
						email=contact.ChildNodes[1].InnerText;
					}
				}
			}
			if(contact.ChildNodes.Count>=4){
				if(contact.ChildNodes[2].InnerText.Length>=7&&contact.ChildNodes[3].InnerText.Length>=2){
					if(contact.ChildNodes[2].InnerText.Substring(0,5)=="Email"){
						email=contact.ChildNodes[3].InnerText;
					}
				}
			}
			if(contact.ChildNodes.Count>=3){
				if(contact.ChildNodes[2].InnerText.Length>=7){
					if(contact.ChildNodes[2].InnerText.Substring(0,6)=="Office"){
						office=contact.ChildNodes[2].InnerText.Substring(8);
					}
				}
			}
			if(contact.ChildNodes.Count>=6){
				if(contact.ChildNodes[5].InnerText.Length>8){
					if(contact.ChildNodes[5].InnerText.Substring(0,6)=="Office"){
						office=contact.ChildNodes[5].InnerText.Substring(8);
					}
				}
			}
			name=article.ChildNodes[1].ChildNodes[1].InnerText;
			title=article.ChildNodes[1].ChildNodes[5].InnerText;

			title=title.Replace("&amp;","&");
			title=title.Replace("&#039;","\'");

			title=WWW.UnEscapeURL(title);
			Dictionary<String,String> faculty_member_data=new Dictionary<String,String>();
			faculty_member_data.Add("name",name);
			faculty_member_data.Add("title",title);
			faculty_member_data.Add("phone",phone);
			faculty_member_data.Add("email",email);
			faculty_member_data.Add("office",office);
			faculty_member_data.Add("link",link);
			faculty_member_data.Add("image",image);
			faculty_data.Add (faculty_member_data);
		}
	}
}

//END ACCESSORY CLASSES
//THREAD SMART QUEUE

public class ThreadSmartQueue
{
	public static readonly object _locker = new object();
	public static Worker[] _workers;
	protected internal static Queue<Job> _jobQ = new Queue<Job>();
	public static ThreadSmartQueue _thread_smart_queue;

	public ThreadSmartQueue (int workerCount)
	{
		_workers = new Worker [workerCount];
		// Create and start a separate thread for each worker
		for (int i = 0; i < workerCount; i++){
			_workers [i] = new Worker(i);
		}
	}

	public void Shutdown (bool waitForWorkers)
	{
		// Enqueue one null item per worker to make each exit.
		foreach (Worker worker in _workers)
			EnqueueItem (null);

		// Wait for workers to finish
		if (waitForWorkers)
			foreach (Worker worker in _workers)
				worker.Join();
	}

	public void EnqueueItem (Job job)
	{
		lock (_locker)
		{
			_jobQ.Enqueue (job);
			job._status=Job.Status.QUEUED;
			Monitor.Pulse(_locker);
		}
	}

	protected internal static void Consume()
	{
		while (true)
		{
			Job job;
			lock (_locker)
			{
				while (ThreadSmartQueue._jobQ.Count == 0){
					Monitor.Wait(_locker);
				}
				job = ThreadSmartQueue._jobQ.Dequeue();

				if (job == null){
					return;
				}
				job._worker=get_worker_from_thread(Thread.CurrentThread);
				job._status=Job.Status.RUNNING;
				job.Work();
				job._status=Job.Status.COMPLETED;
				job.OnComplete();
			}
		}
	}
	public static Worker get_worker_from_thread(Thread t){
		Worker worker=null;

		foreach(Worker w in _workers){
			if(w.thread==t){
				worker=w;
				break;
			}
		}

		return worker;
	}
}

public abstract class Job{ // Extend this class
	public Worker _worker;
	public Status _status;
	public enum Status{
		CREATED,
		QUEUED,
		RUNNING,
		COMPLETED
	}
	public Job(){
		_status=Status.CREATED;
	}

	protected internal virtual void Work(){}

	protected internal virtual void OnComplete(){}

	public IEnumerator WaitFor() //StartCoroutine(WaitOnJobMethod(job)); - yield return StartCoroutine(job.WaitFor());
	{
		while(_status!=Status.COMPLETED)
		{
			yield return null;
		}
	}
}

public class Worker
{
	private Thread _thread = null;
	public Thread thread{
		get{
			return _thread;
		}
	}
	private int worker_number;
	public int WorkerNumber{
		get{
			return worker_number;
		}
	}

	public Worker(int i){
		worker_number=i;
		_thread=new Thread (ThreadSmartQueue.Consume);
		_thread.Priority=System.Threading.ThreadPriority.Lowest;
		_thread.Start();
	}

	protected internal void Join(){
		_thread.Join();
	}

	protected internal virtual void Abort()
	{
		_thread.Abort();
	}
}

//END THREAD SMART QUEUE
//BASE CLASSES

public class BaseObject : MonoBehaviour{


}

public class Button : BaseObject {

	
	void Awake(){
		
	}
	
	void Update(){
		
	}
}

//END BASE CLASSES

//DEPENDENCIES
public static class HtmlToText
{

	public static string Convert(string path)
	{
		HtmlDocument doc = new HtmlDocument();
		doc.Load(path);
		return ConvertDoc(doc);
	}

	public static string ConvertHtml(string html)
	{
		HtmlDocument doc = new HtmlDocument();
		doc.LoadHtml(html);
		return ConvertDoc(doc);
	}

	public static string ConvertDoc (HtmlDocument doc)
	{
		using (StringWriter sw = new StringWriter())
		{
			ConvertTo(doc.DocumentNode, sw);
			sw.Flush();
			return sw.ToString();
		}
	}

	internal static void ConvertContentTo(HtmlNode node, TextWriter outText, PreceedingDomTextInfo textInfo)
	{
		foreach (HtmlNode subnode in node.ChildNodes)
		{
			ConvertTo(subnode, outText, textInfo);
		}
	}
	public static void ConvertTo(HtmlNode node, TextWriter outText)
	{
		ConvertTo(node, outText, new PreceedingDomTextInfo(false));
	}
	internal static void ConvertTo(HtmlNode node, TextWriter outText, PreceedingDomTextInfo textInfo)
	{
		string html;
		switch (node.NodeType)
		{
		case HtmlNodeType.Comment:
			// don't output comments
			break;
		case HtmlNodeType.Document:
			ConvertContentTo(node, outText, textInfo);
			break;
		case HtmlNodeType.Text:
			// script and style must not be output
			string parentName = node.ParentNode.Name;
			if ((parentName == "script") || (parentName == "style"))
			{
				break;
			}
			// get text
			html = ((HtmlTextNode)node).Text;
			// is it in fact a special closing node output as text?
			if (HtmlNode.IsOverlappedClosingElement(html))
			{
				break;
			}
			// check the text is meaningful and not a bunch of whitespaces
			if (html.Length == 0)
			{
				break;
			}
			if (!textInfo.WritePrecedingWhiteSpace || textInfo.LastCharWasSpace)
			{
				html= html.TrimStart();
				if (html.Length == 0) { break; }
				textInfo.IsFirstTextOfDocWritten.Value = textInfo.WritePrecedingWhiteSpace = true;
			}
			outText.Write(HtmlEntity.DeEntitize(Regex.Replace(html.TrimEnd(), @"\s{2,}", " ")));
			if (textInfo.LastCharWasSpace = char.IsWhiteSpace(html[html.Length - 1]))
			{
				outText.Write(' ');
			}
			break;
		case HtmlNodeType.Element:
			string endElementString = null;
			bool isInline;
			bool skip = false;
			int listIndex = 0;
			switch (node.Name)
			{
			case "nav":
				skip = true;
				isInline = false;
				break;
			case "body":
			case "section":
			case "article":
			case "aside":
			case "h1":
			case "h2":
			case "header":
			case "footer":
			case "address":
			case "main":
			case "div":
			case "p": // stylistic - adjust as you tend to use
				if (textInfo.IsFirstTextOfDocWritten)
				{
					outText.Write("\r\n");
				}
				endElementString = "\r\n";
				isInline = false;
				break;
			case "br":
				outText.Write("\r\n");
				skip = true;
				textInfo.WritePrecedingWhiteSpace = false;
				isInline = true;
				break;
			case "a":
				if (node.Attributes.Contains("href"))
				{
					string href = node.Attributes["href"].Value.Trim();
					if (node.InnerText.IndexOf(href, StringComparison.InvariantCultureIgnoreCase)==-1)
					{
						endElementString =  "<" + href + ">";
					}  
				}
				isInline = true;
				break;
			case "li": 
				if(textInfo.ListIndex>0)
				{
					outText.Write("\r\n{0}.\t", textInfo.ListIndex++); 
				}
				else
				{
					outText.Write("\r\n*\t"); //using '*' as bullet char, with tab after, but whatever you want eg "\t->", if utf-8 0x2022
				}
				isInline = false;
				break;
			case "ol": 
				listIndex = 1;
				goto case "ul";
			case "ul": //not handling nested lists any differently at this stage - that is getting close to rendering problems
				endElementString = "\r\n";
				isInline = false;
				break;
			case "img": //inline-block in reality
				if (node.Attributes.Contains("alt"))
				{
					outText.Write('[' + node.Attributes["alt"].Value);
					endElementString = "]";
				}
				if (node.Attributes.Contains("src"))
				{
					outText.Write('<' + node.Attributes["src"].Value + '>');
				}
				isInline = true;
				break;
			default:
				isInline = true;
				break;
			}
			if (!skip && node.HasChildNodes)
			{
				ConvertContentTo(node, outText, isInline ? textInfo : new PreceedingDomTextInfo(textInfo.IsFirstTextOfDocWritten){ ListIndex = listIndex });
			}
			if (endElementString != null)
			{
				outText.Write(endElementString);
			}
			break;
		}
	}
}
internal class PreceedingDomTextInfo
{
	public PreceedingDomTextInfo(BoolWrapper isFirstTextOfDocWritten)
	{
		IsFirstTextOfDocWritten = isFirstTextOfDocWritten;
	}
	public bool WritePrecedingWhiteSpace {get;set;}
	public bool LastCharWasSpace { get; set; }
	public readonly BoolWrapper IsFirstTextOfDocWritten;
	public int ListIndex { get; set; }
}
internal class BoolWrapper
{
	public BoolWrapper() { }
	public bool Value { get; set; }
	public static implicit operator bool(BoolWrapper boolWrapper)
	{
		return boolWrapper.Value;
	}
	public static implicit operator BoolWrapper(bool boolWrapper)
	{
		return new BoolWrapper{ Value = boolWrapper };
	}
}

// The RequestState class passes data across async calls.
public class RequestState
{
	const int BufferSize = 1024;
	public byte[] BufferRead;
	public byte[] data;
	public WebRequest Request;
	public WebResponse Response;
	public byte[] requestData;
	public string errorMessage;
	// Create Decoder for appropriate enconding type.
	public bool timedOut=false;

	public RequestState()
	{
		requestData = new byte[0];
		BufferRead = new byte[BufferSize];
		Request = null;
		Response = null;
		errorMessage=null;
	}     
}

public class WebAsync {
	const int TIMEOUT = 10; // seconds

	public bool isResponseCompleted = false;
	public RequestState requestState;

	public bool isURLcheckingCompleted = false;
	public bool isURLmissing = false;

	/// <summary>
	/// Updates the isURLmissing parameter.
	/// If the URL returns 404 or the connection is broken, it's missing. Else, we suppose it's fine.
	/// This should or can be used along with web async instance's isURLcheckingCompleted parameter
	/// inside a IEnumerator method capable of yield return for it, although it's mostly for clarity.
	/// Here's an usage example:
	/// 
	/// WebAsync webAsync = new WebAsync(); StartCoroutine( webAsync.CheckForMissingURL(url) );
	/// while (! webAsync.isURLcheckingCompleted) yield return null;
	/// bool result = webAsync.isURLmissing;
	/// 
	/// </summary>
	/// <param name='url'>
	/// A fully formated URL.
	/// </param>
	public IEnumerator CheckForMissingURL (string url) {
		isURLcheckingCompleted = false;
		isURLmissing = false;

		Uri httpSite = new Uri(url);
		WebRequest webRequest = WebRequest.Create(httpSite);

		// We need no more than HTTP's head
		webRequest.Method = "HEAD";

		// Get the request's reponse
		requestState = null;

		// Manually iterate IEnumerator, because Unity can't do it (and this does not inherit StartCoroutine from MonoBehaviour)
		IEnumerator e = GetResponse(webRequest);
		while (e.MoveNext()) yield return e.Current;
		while (! isResponseCompleted) yield return null; // this while is just to be sure and clear

		// Deal up with the results
		if (requestState.errorMessage != null) {
			if ( requestState.errorMessage.Contains("404") || requestState.errorMessage.Contains("NameResolutionFailure") ) {
				isURLmissing = true;
			} else {
				UnityEngine.Debug.LogError("[WebAsync] Error trying to verify if URL '"+ url +"' exists: "+ requestState.errorMessage);
			}
		}

		isURLcheckingCompleted = true;
	}

	/// <summary>
	/// Equivalent of webRequest.GetResponse, but using our own RequestState.
	/// This can or should be used along with web async instance's isResponseCompleted parameter
	/// inside a IEnumerator method capable of yield return for it, although it's mostly for clarity.
	/// Here's an usage example:
	/// 
	/// WebAsync webAsync = new WebAsync(); StartCoroutine( webAsync.GetReseponse(webRequest) );
	/// while (! webAsync.isResponseCompleted) yield return null;
	/// RequestState result = webAsync.requestState;
	/// 
	/// </summary>
	/// <param name='webRequest'>
	/// A System.Net.WebRequest instanced var.
	/// </param>
	public IEnumerator GetResponse (WebRequest webRequest) {
		isResponseCompleted = false;
		requestState = new RequestState();

		// Put the request into the state object so it can be passed around
		requestState.Request = webRequest;

		// Do the actual async call here
		IAsyncResult asyncResult = (IAsyncResult) webRequest.BeginGetResponse(
			new AsyncCallback(RespCallback), requestState);

		// WebRequest timeout won't work in async calls, so we need this instead
		ThreadPool.RegisterWaitForSingleObject(
			asyncResult.AsyncWaitHandle,
			new WaitOrTimerCallback(ScanTimeoutCallback),
			requestState,
			(TIMEOUT *1000), // obviously because this is in miliseconds
			true
		);

		// Wait until the the call is completed
		while (!asyncResult.IsCompleted) { yield return null; }

		// Help debugging possibly unpredictable results
		if (requestState != null) {
			if (requestState.errorMessage != null) {
				// this is not an ERROR because there are at least 2 error messages that are expected: 404 and NameResolutionFailure - as can be seen on CheckForMissingURL
				UnityEngine.Debug.Log("[WebAsync] Error message while getting response from request '"+ webRequest.RequestUri.ToString() +"': "+ requestState.errorMessage);
			}
		}

		isResponseCompleted = true;
	}

	private void RespCallback (IAsyncResult asyncResult)
	{
		WebRequest webRequest = requestState.Request;

		try {
			requestState.Response = webRequest.EndGetResponse(asyncResult);
		} catch (WebException webException) {
			requestState.errorMessage = "From callback, "+ webException.Message;
		}
	}

	private void ScanTimeoutCallback (object state, bool timedOut)
	{
		if (timedOut)  {
			RequestState requestState = (RequestState)state;
			if (requestState != null) 
				requestState.timedOut=true;
				requestState.Request.Abort();
		} else {
			RegisteredWaitHandle registeredWaitHandle = (RegisteredWaitHandle)state;
			if (registeredWaitHandle != null)
				registeredWaitHandle.Unregister(null);
		}
	}
}

//END DEPENDENCIES