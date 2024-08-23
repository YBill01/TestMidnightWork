using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Reflection;

#if UNITY_TOOLBAR_EXTENDER
using UnityToolbarExtender;
using UnityEngine.SceneManagement;
using DG.Tweening.Plugins.Core.PathCore;

#else
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
#endif
#if UNITY_2019_1_OR_NEWER
using VisualElement = UnityEngine.UIElements.VisualElement;
#else
using VisualElement = UnityEngine.Experimental.UIElements.VisualElement;
#endif

namespace ASze.CustomPlayButton
{
	[InitializeOnLoad]
	public static class CustomPlayButton
	{
#if UNITY_TOOLBAR_EXTENDER
		const string FOLDER_PATH = "Assets/Plugins/CustomPlayButton/Editor/";
		const string SETTING_PATH = FOLDER_PATH + "BookmarkSetting.asset";
		const string ICONS_PATH = FOLDER_PATH + "Icons/";//"Packages/com.antonysze.custom-play-button/Editor/Icons/";

		private static SceneBookmark bookmark = null;
		private static SceneAsset selectedScene = null;


		static GUIContent customSceneContent;
		static GUIContent gameSceneContent;

		static Rect buttonRect;
		static VisualElement toolbarElement;
		static SceneAsset lastScene = null;

		static bool _firstStart = true;

		public static SceneBookmark Bookmark
		{
			get
			{
				if (bookmark == null)
				{
					bookmark = ScriptableObject.CreateInstance<SceneBookmark>();
					if (!Directory.Exists(FOLDER_PATH))
						Directory.CreateDirectory(FOLDER_PATH);
					AssetDatabase.CreateAsset(bookmark, SETTING_PATH);
					AssetDatabase.Refresh();
				}
				return bookmark;
			}
		}

		public static SceneAsset SelectedScene
		{
			get { return selectedScene; }
			set
			{
				selectedScene = value;
				toolbarElement?.MarkDirtyRepaint();

				if (value != null)
				{
					var path = AssetDatabase.GetAssetPath(value);
					EditorPrefs.SetString(GetEditorPrefKey(), path);
				}
				else
				{
					EditorPrefs.DeleteKey(GetEditorPrefKey());
				}
			}
		}

		static class ToolbarStyles
		{
			public static readonly GUIStyle commandButtonStyle;
			public static readonly GUIStyle commandButtonGamePlayStyle;
			public static readonly GUIStyle commandDropDownStyle;

			static ToolbarStyles()
			{
				EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;

				commandButtonStyle = new GUIStyle("ProjectBrowserHeaderBgTop"/*"IN Title"*//*"toolbarbutton"*//*"ButtonMid"*//*"Command"*/)
				{
					fixedWidth = 28,
					fixedHeight = 18,
					//border = new RectOffset(),
					margin = new RectOffset(1, 1, 0, 0),
					//padding = new RectOffset(),
					//fontSize = 16,
					//alignment = TextAnchor.LowerCenter,
					//imagePosition = ImagePosition.ImageOnly,
					//fontStyle = FontStyle.Bold
				};
				commandButtonGamePlayStyle = new GUIStyle(commandButtonStyle)
				{
					fixedWidth = 48,
					//contentOffset = new Vector2(4, 0)
				};
				commandDropDownStyle = new GUIStyle("DropDown"/*"ToolbarPopup"*/)
				{
					fixedWidth = 128,
					fixedHeight = 20,
					margin = new RectOffset(1, 1, -1, 0),
					//overflow = new RectOffset(0,0,0,0),
					//padding = new RectOffset(0,0,0,0),
					//border = new RectOffset(0,0,0,0),
				};
			}
		}

		static CustomPlayButton()
		{
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarLeftGUI);
			EditorApplication.update += OnUpdate;

			if (bookmark == null)
			{
				bookmark = AssetDatabase.LoadAssetAtPath<SceneBookmark>(SETTING_PATH);
				Bookmark?.RemoveNullValue();
			}

			var savedScenePath = EditorPrefs.GetString(GetEditorPrefKey(), "");
			selectedScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(savedScenePath);
			if (selectedScene == null && EditorBuildSettings.scenes.Length > 0)
			{
				var scenePath = EditorBuildSettings.scenes[0].path;
				SelectedScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
			}

			customSceneContent = CreateIconContent("PlaySceneButton.png", "d_UnityEditor.Timeline.TimelineWindow@2x", "Play Custom Scene");
			gameSceneContent = CreateIconContent("PlayGameButton.png", "d_UnityEditor.GameView@2x", "Play Game Scene");
		}

		/*public static void Init()
		{
			customSceneContent = CreateIconContent("PlaySceneButton.png", "d_UnityEditor.Timeline.TimelineWindow@2x", "Play Custom Scene");
			gameSceneContent = CreateIconContent("PlayGameButton.png", "d_UnityEditor.GameView@2x", "Play Game Scene");
		}*/

		static void OnToolbarLeftGUI()
		{
			GUILayout.FlexibleSpace();
			var sceneName = selectedScene != null ? selectedScene.name : "Select Scene...";
			var selected = EditorGUILayout.DropdownButton(new GUIContent(sceneName), FocusType.Passive, ToolbarStyles.commandDropDownStyle/*new GUIStyle("DropDown"*//*"Button"*//*), GUILayout.Width(128.0f)*//*, GUILayout.Height(20.0f)*/);
			if (Event.current.type == EventType.Repaint)
			{
				buttonRect = GUILayoutUtility.GetLastRect();
			}

			if (selected)
			{
				PopupWindow.Show(buttonRect, new EditorSelectScenePopup());
			}

			GUI.enabled = !EditorApplication.isPlaying;
			if (!EditorApplication.isPlaying)
			{
				GUI.enabled = EditorSceneManager.GetActiveScene().name != selectedScene.name;
				if (GUI.enabled)
				{
					string path = SceneUtility.GetScenePathByBuildIndex(0);
#if UNITY_2021_1_OR_NEWER
					string name = path[..^6][(path.LastIndexOf('/') + 1)..];
#else
					string name = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);
#endif
					if (selectedScene.name == name)
					{
						GUI.color = new Color(1.5f, 1.5f, 1.5f, 1.0f);
					}
				}
			}
			if (GUILayout.Button(customSceneContent, ToolbarStyles.commandButtonStyle))
			{
				if (selectedScene != null)
				{
					StartScene(selectedScene);
				}
				else
				{
					EditorUtility.DisplayDialog(
						"Cannot play custom scene",
						"No scene is selected to play. Please select a scene from the dropdown list.",
						"Ok");
				}
			}
			//GUILayout.Space(4);
			if (!EditorApplication.isPlaying)
			{
				GUI.enabled = EditorSceneManager.GetActiveScene().buildIndex != 0;
				if (GUI.enabled)
				{
					GUI.color = new Color(1.5f, 1.5f, 1.5f, 1.0f);
				}
			}
			if (GUILayout.Button(gameSceneContent, ToolbarStyles.commandButtonGamePlayStyle))
			{
				if (EditorBuildSettings.scenes.Length > 0)
				{
					var scenePath = EditorBuildSettings.scenes[0].path;
					var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
					StartScene(scene);
				}
				else
				{
					if (!EditorUtility.DisplayDialog(
						"Cannot play the game",
						"Please add the first scene in build setting in order to play the game.",
						"Ok", "Open build setting"))
					{
						EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
					}
					// Avoid error from GUILayout.EndHorizontal()
					GUILayout.BeginHorizontal();
				}
			}
			GUI.color = Color.white;
			GUI.enabled = true;
		}

		static void StartScene(SceneAsset scene)
		{
			if (EditorApplication.isPlaying)
			{
				lastScene = scene;
				EditorApplication.isPlaying = false;
			}
			else
			{
				ChangeScene(scene);
			}
		}

		static void OnUpdate()
		{
			//fix first start editor load icons...
			if (_firstStart)
			{
				customSceneContent = CreateIconContent("PlaySceneButton01.png", "d_UnityEditor.Timeline.TimelineWindow@2x", "Play Custom Scene");
				gameSceneContent = CreateIconContent("PlayGameButton01.png", "d_UnityEditor.GameView@2x", "Play Game Scene");
				_firstStart = false;
			}

			// Get toolbar element for repainting
			if (toolbarElement == null)
			{
				var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
				var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
				var currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
				if (currentToolbar != null)
				{
					var guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
#if UNITY_2020_1_OR_NEWER
					var iWindowBackendType = typeof(Editor).Assembly.GetType("UnityEditor.IWindowBackend");
					var guiBackend = guiViewType.GetProperty("windowBackend",
						BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					var viewVisualTree = iWindowBackendType.GetProperty("visualTree",
						BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					var windowBackend = guiBackend.GetValue(currentToolbar);
					toolbarElement = (VisualElement)viewVisualTree.GetValue(windowBackend, null);
#else
                    var viewVisualTree = guiViewType.GetProperty("visualTree",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    toolbarElement = (VisualElement)viewVisualTree.GetValue(currentToolbar, null);
#endif
				}
			}

			if (lastScene == null ||
				EditorApplication.isPlaying || EditorApplication.isPaused ||
				EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			ChangeScene(lastScene);
			lastScene = null;
		}

		static void ChangeScene(SceneAsset scene)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				EditorSceneManager.playModeStartScene = scene;
				EditorApplication.isPlaying = true;
			}
		}

		static void HandleOnPlayModeChanged(PlayModeStateChange playMode)
		{
			if (playMode == PlayModeStateChange.ExitingPlayMode)
			{
				EditorSceneManager.playModeStartScene = null;
			}
		}

		public static string GetEditorPrefKey()
		{
			var projectPrefix = PlayerSettings.companyName + "." + PlayerSettings.productName;
			return projectPrefix + "_CustomPlayButton_SelectedScenePath";
		}

		public static GUIContent CreateIconContent(string localTex, string builtInTex, string tooltip)
		{
			var tex = LoadTexture(localTex);
			if (tex != null) return new GUIContent(tex, tooltip);
			else return EditorGUIUtility.IconContent(builtInTex, tooltip);
		}

		public static Texture2D LoadTexture(string path)
		{
			return AssetDatabase.LoadAssetAtPath<Texture2D>(ICONS_PATH + path);
			//return (Texture2D)EditorGUIUtility.Load(ICONS_PATH + path);
		}

		/*class AllPostprocessor : AssetPostprocessor
		{
			static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
			{
				Init();
			}
		}*/
#else
        static AddRequest request;

        static CustomPlayButton()
        {
            if (!EditorUtility.DisplayDialog(
                "Cannot activate Custom Play Button",
                "Prerequisite package is needed for \"unity-custom-play-button\".\nPlease install package \"unity-toolbar-extender\"(https://github.com/marijnz/unity-toolbar-extender.git).",
                "Ok", "Install package"))
            {
                request = Client.Add("https://github.com/marijnz/unity-toolbar-extender.git");
                EditorApplication.update += Progress;
            }
        }

        static void Progress()
        {
            if (request.IsCompleted)
            {
                if (request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + request.Result.packageId);
                else if (request.Status >= StatusCode.Failure)
                    Debug.Log(request.Error.message);

                EditorApplication.update -= Progress;
            }
        }
#endif
				}
}