using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour {

	public struct Param {
		public LINE line;
		public float time;

		public Param(LINE line, float time) {
			this.line = line;
			this.time = time;
		}
	}

	public Param param { get; private set; }

	public void SetParam(Param param) {
		this.param = param;
	}
}
