using UnityEngine;

public class UIAppCanvas : MonoBehaviour
{
	[SerializeField]
	private RectTransform fpsComponent;

	private void Awake()
	{
		DontDestroyOnLoad(this);
	}
}