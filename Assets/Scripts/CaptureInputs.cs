using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class CaptureInputs : MonoBehaviour {
	[System.Flags]
	[System.Serializable]
	public enum InputTrend {
		NegativeX, PositiveX, NeutralX,
		NegativeY, PositiveY, NeutralY,
		NegativeButton1, PositiveButton1,
		NegativeButton2, PositiveButton2,
		NegativeButton3, PositiveButton3,
		NegativeButton4, PositiveButton4,
		NegativeButton5, PositiveButton5,
		NegativeButton6, PositiveButton6,
		NegativeButton7, PositiveButton7,
		NegativeButton8, PositiveButton8
	}



	private InputTrend currentFrameInputs {
		get {
			InputTrend _currentInputTrend = 0;



			if (!string.IsNullOrEmpty (inputNames.xAxis)) {
				if (CrossPlatformInputManager.GetAxis (inputNames.xAxis) > 0.0f)
					_currentInputTrend |= InputTrend.PositiveX;
				else if (CrossPlatformInputManager.GetAxis (inputNames.xAxis) < 0.0f)
					_currentInputTrend |= InputTrend.NegativeX;
				else
					_currentInputTrend |= InputTrend.NeutralX;
			}

			if (!string.IsNullOrEmpty (inputNames.yAxis)) {
				if (CrossPlatformInputManager.GetAxis (inputNames.yAxis) > 0.0f)
					_currentInputTrend |= InputTrend.PositiveY;
				else if (CrossPlatformInputManager.GetAxis (inputNames.yAxis) < 0.0f)
					_currentInputTrend |= InputTrend.NegativeY;
				else
					_currentInputTrend |= InputTrend.NeutralY;
			}

			if (!string.IsNullOrEmpty (inputNames.button1)) {
				if (CrossPlatformInputManager.GetButtonDown (inputNames.button1))
					_currentInputTrend |= InputTrend.PositiveButton1;
				else if (CrossPlatformInputManager.GetButtonUp (inputNames.button1))
					_currentInputTrend |= InputTrend.NegativeButton1;
			}

			if (!string.IsNullOrEmpty (inputNames.button2)) {
				if (CrossPlatformInputManager.GetButtonDown (inputNames.button2))
					_currentInputTrend |= InputTrend.PositiveButton2;
				else if (CrossPlatformInputManager.GetButtonUp (inputNames.button2))
					_currentInputTrend |= InputTrend.NegativeButton2;
			}

			if (!string.IsNullOrEmpty (inputNames.button3)) {
				if (CrossPlatformInputManager.GetButtonDown (inputNames.button3))
					_currentInputTrend |= InputTrend.PositiveButton3;
				else if (CrossPlatformInputManager.GetButtonUp (inputNames.button3))
					_currentInputTrend |= InputTrend.NegativeButton3;
			}

			if (!string.IsNullOrEmpty (inputNames.button4)) {
				if (CrossPlatformInputManager.GetButtonDown (inputNames.button4))
					_currentInputTrend |= InputTrend.PositiveButton4;
				else if (CrossPlatformInputManager.GetButtonUp (inputNames.button4))
					_currentInputTrend |= InputTrend.NegativeButton4;
			}

			if (!string.IsNullOrEmpty (inputNames.button5)) {
				if (CrossPlatformInputManager.GetButtonDown (inputNames.button5))
					_currentInputTrend |= InputTrend.PositiveButton5;
				else if (CrossPlatformInputManager.GetButtonUp (inputNames.button5))
					_currentInputTrend |= InputTrend.NegativeButton5;
			}

			if (!string.IsNullOrEmpty (inputNames.button6)) {
				if (CrossPlatformInputManager.GetButtonDown (inputNames.button6))
					_currentInputTrend |= InputTrend.PositiveButton6;
				else if (CrossPlatformInputManager.GetButtonUp (inputNames.button6))
					_currentInputTrend |= InputTrend.NegativeButton6;
			}



			return _currentInputTrend;
		}
	}
	private void RecordInputs (InputTrend _currentInputTrend) {
		InputTrend[] _recordedTrend = new InputTrend[framesToRecord];
		_recordedTrend [0] = _currentInputTrend;
		for (int i = 0; i < _recordedTrend.Length - 1; i++) {
			_recordedTrend [i + 1] = recordedTrend [i];
		}
		recordedTrend = _recordedTrend;
	}
	private InputTrend[] recordedTrend;

	public bool GetInputTrends (InputTrend[][] referenceTrends) {
		bool validTrend = false;
		foreach(InputTrend[] referenceTrend in referenceTrends) {
			if (referenceTrend == recordedTrend) {
				validTrend = true;
				break;
			}
		}
		return validTrend;
	}

	public int framesToRecord;



	[System.Serializable]
	public enum RecordOn {
		DontRecord, FixedUpdate, Update, LateUpdate
	}
	public RecordOn recordOn;

	[System.Serializable]
	public struct InputNames {
		public string xAxis, yAxis, button1, button2, button3, button4, button5, button6, button7, button8;
	} 
	public InputNames inputNames;



	public void Init () {
		recordedTrend = new InputTrend[framesToRecord];
	}

	void Awake () {
		Init ();
	}



	void FixedUpdate () {
		if (recordOn == RecordOn.FixedUpdate)
			RecordInputs (currentFrameInputs);
	}
	void Update () {
		if (recordOn == RecordOn.Update)
			RecordInputs (currentFrameInputs);
	}
	void LateUpdate () {
		if (recordOn == RecordOn.LateUpdate)
			RecordInputs (currentFrameInputs);
	}
}
