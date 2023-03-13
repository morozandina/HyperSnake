using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagment : MonoBehaviour
{
    public Animator transition;
    public void StartGame()
    {
        StartCoroutine(LoadLevel(1));
    }
    public IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene(levelIndex);
    }

    public void GoToScene(int index)
    {
        StartCoroutine(LoadLevel(index));
    }
}
