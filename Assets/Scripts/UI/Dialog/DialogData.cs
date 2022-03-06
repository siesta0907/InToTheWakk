using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DialogElement
{
	public string talker;
	public Sprite portrait;
	[TextArea]
	public string content;
}


[CreateAssetMenu(fileName = "New Dialog Data", menuName = "Create Dialog Data")]
public class DialogData : ScriptableObject
{
	public DialogElement[] scripts;
}
