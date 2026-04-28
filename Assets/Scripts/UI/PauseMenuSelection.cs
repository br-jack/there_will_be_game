using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuSelection : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button firstSelectedButton;

    private Coroutine selectRoutine;

    public void SelectFirstButton()
    {
        if (selectRoutine != null)
        {
            StopCoroutine(selectRoutine);
        }

        selectRoutine = StartCoroutine(SelectFirstButtonNextFrame());
    }

    private IEnumerator SelectFirstButtonNextFrame()
    {
        yield return null;

        if (pauseMenuPanel == null || firstSelectedButton == null)
        {
            yield break;
        }

        if (!pauseMenuPanel.activeInHierarchy)
        {
            yield break;
        }

        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
    }

    public void ClearSelection()
    {
        if (selectRoutine != null)
        {
            StopCoroutine(selectRoutine);
            selectRoutine = null;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
