using System.Collections.Generic;
using UnityEngine;
using ANFW.Tools;
using UnityEngine.UI;

namespace ANFW.Sample
{
    public class SampleCsvRead : MonoBehaviour
    {
        public List<Character> characterList;
        public List<Dictionary<string, string>> characterDataList;

        async void Start()
        {
            this.characterList = await CsvReader.getClass<Character>("Sample/Character");
            this.characterDataList = await CsvReader.getList("Sample/Character");

            GameObject canvas = GameObject.Find("Canvas");
            int i = 0;
            foreach (Dictionary<string, string> character in this.characterDataList) {
                GameObject panel = canvas.transform.GetChild(i).gameObject;
                foreach ((string key, string value) in character) {
                    panel.transform.Find(key).GetComponent<Text>().text = value;
                }
                panel.SetActive(true);
                i++;
            }
        }
    }
}