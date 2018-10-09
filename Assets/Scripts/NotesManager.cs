using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotesManager : MonoBehaviour {

	public static readonly string SAVE_KEY = "key:score";

	private static readonly float STANDARD_SPEED = 10f;
	private static readonly int DRAW_COMBO = 5;

	// 出現位置
	private static readonly Dictionary<LINE, float> NOTES_LINE_POSITION = new Dictionary<LINE, float>() {
		{ LINE.LINE_1, -6f },
		{ LINE.LINE_2, -2f },
		{ LINE.LINE_3, 2f },
		{ LINE.LINE_4, 6f },
	};

	// 譜面データ
	private static readonly List<Note.Param> NOTES_MASTER_DATA = new List<Note.Param>() {
		{ new Note.Param(LINE.LINE_1, 1f ) },
		{ new Note.Param(LINE.LINE_2, 2f ) },
		{ new Note.Param(LINE.LINE_3, 3f ) },
		{ new Note.Param(LINE.LINE_4, 4f ) },
		{ new Note.Param(LINE.LINE_1, 5f ) },
		{ new Note.Param(LINE.LINE_2, 6f ) },
		{ new Note.Param(LINE.LINE_3, 7f ) },
		{ new Note.Param(LINE.LINE_4, 8f ) },
	};

	// 判定誤差
	private static readonly Dictionary<GRADE, float> JUDGE_TABLE = new Dictionary<GRADE, float>() {
		{ GRADE.PERFECT, 0.1f },
		{ GRADE.GREAT, 0.2f },
		{ GRADE.GOOD, 0.3f },
		{ GRADE.MISS, 0.6f },
	};

	// 判定の色
	private static readonly Dictionary<GRADE, Color> JUDGE_COLOR_TABLE = new Dictionary<GRADE, Color>() {
		{ GRADE.PERFECT, Color.yellow },
		{ GRADE.GREAT, Color.green },
		{ GRADE.GOOD, Color.blue },
		{ GRADE.MISS, Color.red },
	};

	// スコア
	private static readonly Dictionary<GRADE, int> SCORE_TABLE = new Dictionary<GRADE, int>() {
		{ GRADE.PERFECT, 1000 },
		{ GRADE.GREAT, 500 },
		{ GRADE.GOOD, 100 },
		{ GRADE.MISS, 0 },
	};

	[SerializeField] private AudioSource bgm;
	[SerializeField] private AudioSource se;
	[SerializeField] private GameObject notePrefab;
	[SerializeField] private GameObject barObj;
	[SerializeField] private JudgeText[] judgeTexts;
	[SerializeField] private ComboText comboText;
	[SerializeField] private Text scoreText;
	[SerializeField] private float hiSpeed = 1;

	private Dictionary<LINE, List<Note>> notesDic = new Dictionary<LINE, List<Note>> ();
	private Dictionary<LINE, int> currentNotesIndexDic = new Dictionary<LINE, int> ();
	private int score = 0;
	private int combo = 0;

	void Awake() {
		for (int i = 0; i < (int)LINE.MAX; ++i) {
			notesDic [(LINE)i] = new List<Note> ();
			currentNotesIndexDic [(LINE)i] = -1;
		}
	}

	// Use this for initialization
	void Start () {
		foreach (var noteData in NOTES_MASTER_DATA) {
			var obj = Instantiate (notePrefab, transform);
			var pos = obj.transform.position;
			pos.x = NOTES_LINE_POSITION [noteData.line];
			pos.y = barObj.transform.position.y + noteData.time * STANDARD_SPEED * hiSpeed;
			obj.transform.position = pos;

			if (currentNotesIndexDic[noteData.line] < 0) {
				currentNotesIndexDic [noteData.line] = 0;
			}

			var note = obj.GetComponent<Note> ();
			note.SetParam (noteData);
			notesDic [noteData.line].Add (note);
		}

		bgm.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!bgm.isPlaying) {
			Finish ();
		}

		#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Return)) {
			Finish ();
		}
		#endif

		foreach (var noteList in notesDic.Values) {
			foreach(var note in noteList) {
				var pos = note.transform.position;
				pos.y = barObj.transform.position.y + (note.param.time - bgm.time) * STANDARD_SPEED * hiSpeed;
				note.transform.position = pos;
			}
		}

		JudgeMiss ();

		if (Input.GetKeyDown(KeyCode.A)) {			
			TapAction (LINE.LINE_1);
		}
		if (Input.GetKeyDown(KeyCode.S)) {
			TapAction (LINE.LINE_2);
		}
		if (Input.GetKeyDown(KeyCode.D)) {
			TapAction (LINE.LINE_3);
		}
		if (Input.GetKeyDown(KeyCode.F)) {
			TapAction (LINE.LINE_4);
		}
	}

	void TapAction(LINE line) {
		se.Play ();

		if (currentNotesIndexDic [line] < 0)
			return;

		CalcScore(line);
	}

	void CalcScore(LINE line) {
		int currentIndex = currentNotesIndexDic [line];
		var currentNote = notesDic [line] [currentIndex];

		float diff = Mathf.Abs (bgm.time - currentNote.param.time);
		GRADE grade = Judge (diff);
		score += SCORE_TABLE [grade];
		if (grade != GRADE.MISS) {
			++combo;
		}
		else {
			combo = 0;
		}

		judgeTexts [(int)line].SetText (grade.ToString (), JUDGE_COLOR_TABLE [grade]);
		scoreText.text = "Score:" + score.ToString ("D7");
		if (combo >= DRAW_COMBO) {
			comboText.SetText(combo.ToString());
		}

		if (currentIndex + 1 < notesDic[line].Count) {
			++currentNotesIndexDic [line];
		} else {
			currentNotesIndexDic [line] = -1;
		}
	}

	GRADE Judge(float diff) {
		GRADE grade = GRADE.MISS;

		if (diff < JUDGE_TABLE[GRADE.PERFECT]) {
			grade = GRADE.PERFECT;
		} else if (diff < JUDGE_TABLE[GRADE.GREAT]) {
			grade = GRADE.GREAT;
		} else if (diff < JUDGE_TABLE[GRADE.GOOD]) {
			grade = GRADE.GOOD;
		}

		return grade;
	}

	void JudgeMiss() {
		for (int i = 0; i < currentNotesIndexDic.Count; ++i) {
			LINE line = (LINE)i;
			int currentIndex = currentNotesIndexDic [line];

			if (currentIndex < 0)
				continue;

			var currentNote = notesDic [line] [currentIndex];

			float diff = bgm.time - currentNote.param.time;
			if (diff <= JUDGE_TABLE[GRADE.MISS]) {
				continue;
			}

			combo = 0;
			judgeTexts [(int)line].SetText (GRADE.MISS.ToString (), JUDGE_COLOR_TABLE [GRADE.MISS]);

			if (currentIndex + 1 < notesDic[line].Count) {
				++currentNotesIndexDic [line];
			} else {
				currentNotesIndexDic [line] = -1;
			}
		}
	}

	private void Finish() {
		PlayerPrefs.SetInt (SAVE_KEY, score);
		GameSceneController.ChangeScene ();
	}
}
