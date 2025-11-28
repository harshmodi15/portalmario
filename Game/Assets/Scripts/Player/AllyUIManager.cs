using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class AllyUIManager : MonoBehaviour
{
    public TextMeshProUGUI allyText;
    private int captured = 0;
    private int max = 1;

    private void Start()
    {
        UpdateAllyText();
    }

    public void AllyCaptured()
    {
        captured = Mathf.Min(captured + 1, max);
        UpdateAllyText();
    }

    public void ReleaseAlly()
    {
        captured = Mathf.Max(captured - 1, 0);
        UpdateAllyText();
    }  
    
    private void UpdateAllyText()
    {
        allyText.text = $"Ally: {captured}/{max}";
    }
}
