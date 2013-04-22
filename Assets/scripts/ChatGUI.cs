using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Pomelo.DotNetClient;

	public class ChatGUI : MonoBehaviour
	{
		private string userName = LoginGUI.userName;
		private PomeloClient pclient = LoginGUI.pc;
	
		private Vector3 chatScrollPosition;
		private Vector3 userScrollPosition;
		
		private string playerName;
		
		private Rect chatWindow;
	
		private Rect usersWindow;
	
		public GUIStyle pomelo_label_Style;
		
		public static string inputField = "";
		
		private ArrayList chatRecords = null; 
		private ArrayList userList = null; 
	
		public bool debug = true;
	
		void Start() 
	    {
			chatRecords = new ArrayList();
			userList = new ArrayList();
			chatWindow = new Rect(0, 0, Screen.width - 150, Screen.height);
			usersWindow = new Rect(Screen.width - 150, 0, 200, Screen.height);
			pomelo_label_Style.normal.textColor = Color.black;
		
			InitUserWindow();
		
			pclient.on("onAdd", (data) => {
				RefreshUserWindow("add", data);
			});
			
			pclient.on("onLeave", (data) => {
				RefreshUserWindow("leave", data);
			});
		
			pclient.on("onChat", (data)=> {
				addMessage(data);
			});
		
	    }
		
		//When quit, release resource
		void Update(){
			if(Input.GetKey(KeyCode.Escape) || Input.GetKey("escape")) {
				if (pclient != null) {
					pclient.disconnect();
				}
				Application.Quit();
			}
			
		}
	
		//When quit, release resource
		void OnApplicationQuit(){
			if (pclient != null) {
					pclient.disconnect();
			}
		}
		
		
		void OnGUI(){
			GUI.backgroundColor = Color.grey;
			chatWindow = GUI.Window(1, chatWindow, GlobalChatWindow, "");
			usersWindow = GUI.Window(2, usersWindow, GlobalUsersWindow, "");
		}
	
		//The window for userlist
		void GlobalUsersWindow(int id) {
			userScrollPosition = GUILayout.BeginScrollView (userScrollPosition);
		
			foreach (string userName in userList) {
				GUILayout.BeginHorizontal();
				GUILayout.Label(userName, pomelo_label_Style);
				GUILayout.EndHorizontal();
			}
		
			GUILayout.EndScrollView ();
		}
	
		//The window for chat records.
		void GlobalChatWindow(int id){
			chatScrollPosition = GUILayout.BeginScrollView (chatScrollPosition);
		
			foreach (ChatRecord cr in chatRecords) {
				GUILayout.BeginHorizontal();
				GUILayout.Label(cr.name + ": " + cr.dialog,pomelo_label_Style);
				GUILayout.EndHorizontal();
	        	GUILayout.Space(3);
			}
		
			GUILayout.EndScrollView ();
		
			if (Event.current.type.ToString() == EventType.keyDown.ToString() && Event.current.character.ToString() == "\n" && inputField.Length > 0)
		    {
				//if (Event.current.type.ToString() == EventType.mouseDown.ToString() && Event.current.character)
		        HitEnter();
		
		    }
			inputField = GUILayout.TextField(inputField);
		}
		
	
		void HitEnter(){
			sendMessage();
			chatScrollPosition.y = 10000000; 
		}
	
		// Init userList and userWindow
		void InitUserWindow(){
			JsonObject jsonObject = LoginGUI.users;
			System.Object users = null;
			if (jsonObject.TryGetValue("users", out users)) {
				string u = users.ToString();
				string [] initUsers = u.Substring(1,u.Length-2).Split(new Char[] { ',' });
				int length = initUsers.Length;
				for(int i = 0; i< length; i++) {
					string s = initUsers[i];
					userList.Add(s.Substring(1, s.Length -2));
				}
			}
		}
	
		//Update the userlist.
		void RefreshUserWindow(string flag,JsonObject msg){
			System.Object user = null;
			if(msg.TryGetValue("user", out user)) {
				if (flag == "add") {
					this.userList.Add(user.ToString());
				} else if (flag == "leave") {
					this.userList.Remove(user.ToString());
				}
			}
		}
	
		//Add message to chat window.
		void addMessage(JsonObject messge) {
			System.Object msg = null, fromName = null, targetName = null;
			if (messge.TryGetValue("msg", out msg) && messge.TryGetValue("from", out fromName) &&
				messge.TryGetValue("target", out targetName)) {
				chatRecords.Add(new ChatRecord(fromName.ToString(), msg.ToString()));
			}
			
		}
	
		void sendMessage(){
			string reg = "^@.*?:";
			if (System.Text.RegularExpressions.Regex.IsMatch(inputField, reg)) {
				solo();
			} else {
				chat("*", inputField);
				inputField = "";
			}
		}
		
		//Chat with someone only.
		void solo(){
			int userL = inputField.IndexOf(":");
			if (userL > 1) {
				string name = inputField.Substring(1, userL-1);
				for(int i = 0; i < userList.Count; i++) {
					if (name == userList[i].ToString()) {
						string con = inputField.Substring(userL + 1, inputField.Length - userL - 1);
						chat (name, con);
						inputField = "@" + name + ":";
						return;
					}
				}
			}
			chat("*", inputField);
			inputField = "";
		}
	
		void chat(string target, string content){
			JsonObject message = new JsonObject();
			message.Add("rid", LoginGUI.channel);
			message.Add("content", content);
			message.Add("from", LoginGUI.userName);
			message.Add("target", target);	
			pclient.request("chat.chatHandler.send", message, (data) => {
				if (target != "*" && target != LoginGUI.userName){
					chatRecords.Add(new ChatRecord(LoginGUI.userName, content));	
				}
			});
		}
		
	}


