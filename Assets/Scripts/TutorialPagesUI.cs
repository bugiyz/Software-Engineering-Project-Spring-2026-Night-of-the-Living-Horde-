using UnityEngine;
using UnityEngine.UI;

// this script is responsible for managing the tutorial pages UI

public class TutorialPagesUI : MonoBehaviour
{
    public GameObject[] pages;
    public Button previousButton;
    public Button nextButton;

    private int currentPage = 0;
    // function to show the current page
    void OnEnable()
    {
        currentPage = 0;
        ShowPage();
    }
    // function to go to the next page
    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            ShowPage();
        }
    }
    // function to go to the previous page
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowPage();
        }
    }
    // function to show the current page
    private void ShowPage()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentPage);
        }

        if (previousButton != null)
        {
            previousButton.interactable = currentPage > 0;
        }

        if (nextButton != null)
        {
            nextButton.interactable = currentPage < pages.Length - 1;
        }
    }
}