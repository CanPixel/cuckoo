﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO.Ports;

public class playerManager : MonoBehaviour {
	public static Dictionary<int, player> leftPlayers = new Dictionary<int, player>();
	public static Dictionary<int, player> rightPlayers = new Dictionary<int, player>();
	public static playerManager self;
	
	public GameObject textPrefab, oilPrefab;

	public bool DebugMode;
	public float frequency;
	public float speed;
	public input[] leftInput;
	public input[] rightInput;

	private SerialPort stream;

	private int[] rotations = new int[6];

	private int lastImpulse = -1;
	private bool shouldImpulse = false;

	[System.Serializable]
	public class input {
		public string name;
		[Range(0,10)]
		public float energy;
		[Range(0,360)]
		public float direction;
		[HideInInspector]
		public float timer;
	};

	public static void addPlayer(bool addToLeft, player p) {
		if(addToLeft) leftPlayers[p.number] = p;
		else rightPlayers[p.number] = p;		
	}

	public string readArduinoInputs(int timeout = 10) {
		stream.ReadTimeout = timeout;
		try {
			return stream.ReadLine();
		}
		catch(System.TimeoutException) {return null;}
	}

	private List<int> temp = new List<int>();
	public void applyInput() {
		string str = "";
		if (!DebugMode){
			str = readArduinoInputs();
			if(str == null) return;
		}

		if(str.StartsWith("D")) {
			int imp;
			bool stri = int.TryParse(str.Split('=')[1], out imp);
			if(!stri) return;
			//Debug.Log(imp);
			if(imp != lastImpulse && !shouldImpulse) shouldImpulse = true;
			if(imp == lastImpulse && shouldImpulse)
			{	
				shouldImpulse = false;
				//leftInput[1].energy += 0.5f;
				foreach(input i in leftInput) i.energy += 0.5f;
				foreach(input i in rightInput) i.energy += 0.5f;
				return;
			}
			lastImpulse = imp;
		}
		else {
			//Debug.Log(str);
			//Direction
			temp.Clear();
			string[] sep = str.Split('|');
			for(int i = 0; i < sep.Length; i++) {
				string part = sep[i].Trim();
				if(part.Length <= 0) continue;
				int w = 0;
				bool l = int.TryParse(part.Substring(3).Trim(), out w);
				if(!l) continue;
				temp.Add(w);
			}
			if (!DebugMode){
				for(int i = 0; i < temp.Count; i++) rotations[i] = temp[i];
				for(int i = 0; i < leftPlayers.Count; i++) leftInput[i].direction = rotations[i] + 90;
				for(int i = 0; i < rightPlayers.Count; i++) rightInput[i].direction = rotations[i+3] - 90;

			}
			
			//Impulses
			for(int i = 0; i < leftPlayers.Count; i++) if(Input.GetKey(leftPlayers[i].keyT)) leftInput[i].energy += 1;
			for(int i = 0; i < rightPlayers.Count; i++) if(Input.GetKey(rightPlayers[i].keyT)) rightInput[i].energy += 1;
		}
	}

	void Awake() {
		self = this;
		if (!DebugMode){
			stream = new SerialPort("COM6", 9600);
			stream.ReadTimeout = 50;
			stream.Open();
		}
		
		
	}

	void Update() {
		applyInput();

		for(int i = 0; i < leftPlayers.Count; i++) {
			player p = leftPlayers[i];
			input inp = leftInput[i];
			if(p) updatePlayer(p,inp);
		}
		for (int i = 0; i < rightPlayers.Count; i++) {
			player p = rightPlayers[i];
			input inp = rightInput[i];
			if(p) updatePlayer(p,inp);
		}
	}

	void updatePlayer(player p, input inp) {
		p.transform.rotation =  Quaternion.Euler(0, inp.direction, 0);
		
		if(inp.energy > 0) {
			inp.timer += Time.deltaTime;
			if (inp.timer > frequency) {
				p.impulse(speed);
				inp.timer = 0;
				inp.energy--;
			}
		}
	}
}
