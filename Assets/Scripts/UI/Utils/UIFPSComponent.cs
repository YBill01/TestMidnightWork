using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UIFPSComponent : MonoBehaviour
{
	public float updateInterval = 0.5f;

	private TMP_Text _countTextField;
	private string _resultText = "{0} FPS";

	private float _accum = 0.0f;
	private int _frames = 0;
	private float _timeleft;
	private float _fps;

	private void Start()
	{
		_countTextField = GetComponent<TMP_Text>();
	}
	private void Update()
	{
		_timeleft -= Time.deltaTime;
		_accum += Time.timeScale / Time.deltaTime;
		_frames++;

		if (_timeleft <= 0.0)
		{
			_fps = (_accum / _frames);
			_timeleft = updateInterval;
			_accum = 0.0f;
			_frames = 0;

			ApplyText();
		}
	}

	private void ApplyText()
	{
		_countTextField.text = string.Format(_resultText, _fps.ToString("F0"));
	}
}