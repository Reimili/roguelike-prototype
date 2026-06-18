using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    public KeyCode reloadKey = KeyCode.R;

    void Update()
    {
        if (Input.GetKeyDown(reloadKey))
        {
            ReloadScene();
        }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
