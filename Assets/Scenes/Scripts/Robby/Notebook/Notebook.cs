using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Notebook : MonoBehaviour
{
    [SerializeField] List<Page> pages = new List<Page>();
    [SerializeField] Page currentPage = new Page();
    [SerializeField] int index = 0;

    [SerializeField] TextMeshPro textMeshPro;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            index--;
            if (index < 0)
                index = 0;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            index++;
            if (index >= pages.Count)
                index = pages.Count - 1;
        }

        if (currentPage != pages[index])
        {
            currentPage = pages[index];
            DisplayPage(currentPage);
        }
    }

    public void DisplayPage(Page page)
    {
        textMeshPro.text = "";

        bool first = true;
        foreach (var note in page.Notes)
        {
            if (!first)
                textMeshPro.text += "\n";
            else
                first = false;

                textMeshPro.text += note;
        }
    }
}

[Serializable]
public class Page
{
    [SerializeField] string[] notes;

    public string[] Notes { get => notes; set => notes = value; }
}
