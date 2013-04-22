using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Pomelo.DotNetClient;

public class LoginGUI : MonoBehaviour {
	public static string userName = "";
	public static string channel = "";
	public static JsonObject users = null;
	
	public static PomeloClient pc = null;
	
	public Texture2D pomelo;
	public GUISkin pomeloSkin; 
	public GUIStyle pomeloStyle;
	
 	void Start() 
    {	
		pomelo = (Texture2D)Resources.Load("pomelo");
		pomeloStyle.normal.textColor = Color.black;
    }
	
	//When quit, release resource
	void Update(){
		if(Input.GetKey(KeyCode.Escape)) {
			if (pc != null) {
				pc.disconnect();
			}
			Application.Quit();
		}
	}
	
	//When quit, release resource
	void OnApplicationQuit(){
		if (pc != null) {
			pc.disconnect();
		}
	}
	
	//Login the chat application and new PomeloClient.
	void Login() {
		string host = "127.0.0.1";
		int port = 3014;
		pc = new PomeloClient(host, port);
		pc.connect(null, (data)=>{
			JsonObject msg = new JsonObject();
			msg["uid"] = userName;
			pc.request("gate.gateHandler.queryEntry", msg, OnQuery);
		});
	}
	
	void OnQuery(JsonObject result){
		if(Convert.ToInt32(result["code"]) == 200){
			pc.disconnect();
			
			string host = (string)result["host"];
			int port = Convert.ToInt32(result["port"]);
			pc = new PomeloClient(host, port);
			pc.connect(null, (data)=>{
				Entry();
			});	
		}
	}
	
	//Entry chat application.
	void Entry(){
		JsonObject userMessage = new JsonObject();
		userMessage.Add("username", userName);
		userMessage.Add("rid", channel);
		if (pc != null) {
			pc.request("connector.entryHandler.enter", userMessage, (data)=>{
				users = data;
				Application.LoadLevel(Application.loadedLevel + 1);
			});
		}
	}
	
	void OnGUI(){
		GUI.skin = pomeloSkin;
		GUI.color = Color.yellow;
		GUI.enabled = true;	
		GUI.Label(new Rect(160, 50, pomelo.width, pomelo.height), pomelo);
		
		GUI.Label(new Rect(75, 350, 50, 20), "name:", pomeloStyle);
		userName = GUI.TextField(new Rect(125, 350, 90, 20), userName);
		GUI.Label(new Rect(225, 350, 55, 20), "channel:", pomeloStyle);
		channel = GUI.TextField(new Rect(280, 350, 100, 20), channel);
		
		if (GUI.Button(new Rect(410, 350, 70, 20), "OK")) {
			if (pc == null) {
				Login();
			}
		}	
	}

 }