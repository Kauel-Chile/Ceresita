using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Paginacion : MonoBehaviour
{
    public Button[] buttonList;
    public int currentIndex = 0;
    public Color nonSelectedColor;
    public Color selectedColor;
    public KProjectManager kProjectManager;

    void Start()
    {
        buttonList = GetComponentsInChildren<Button>();

        ChangeButtonSelected(0);
    }

    void ChangeButtonSelected(int index)
    {
        if(index != currentIndex)
        {
            ColorBlock nonSelectedButtonColorBlock = buttonList[currentIndex].colors;
            nonSelectedButtonColorBlock.pressedColor = nonSelectedColor;
            nonSelectedButtonColorBlock.normalColor = nonSelectedColor;
            nonSelectedButtonColorBlock.highlightedColor = nonSelectedColor;
            buttonList[currentIndex].colors = nonSelectedButtonColorBlock;
        }

        ColorBlock selectedButtonColorBlock = buttonList[index].colors;
        selectedButtonColorBlock.pressedColor = selectedColor;
        selectedButtonColorBlock.normalColor = selectedColor;
        selectedButtonColorBlock.highlightedColor = selectedColor;
        buttonList[index].colors = selectedButtonColorBlock;

        currentIndex = index;
    }

    public void GoToPage(int index)
    {
        ChangeButtonSelected(index);
        kProjectManager.Load(kProjectManager.projectPreviewList.Length * index);
    }
}
