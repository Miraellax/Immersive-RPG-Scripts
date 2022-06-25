using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("dialogue")]
public class Dialogue
{

    [XmlElement("node")]
    public Node[] nodes;

    public static Dialogue Load(TextAsset _xml)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));
        StringReader reader = new StringReader(_xml.text);
        Dialogue dial = serializer.Deserialize(reader) as Dialogue;
        return dial;
    }
}

[System.Serializable]
public class Node
{
    [XmlElement("npctext")]
    public string npcText;

    [XmlArray("answers")]
    [XmlArrayItem("answer")]
    public Answer[] answers;
}

[System.Serializable]
public class Answer
{
    [XmlAttribute("tonode")]
    public int nextNode;
    [XmlElement("text")]
    public string text;
    [XmlElement("dialend")]
    public string end;


    [XmlAttribute("questvalue")]
    public int questValue;
    [XmlAttribute("needquestvalue")]
    public int needQuestValue;
    [XmlElement("questname")]
    public string questName;


    [XmlAttribute("rewardgold")]
    public int rewardGold;
}