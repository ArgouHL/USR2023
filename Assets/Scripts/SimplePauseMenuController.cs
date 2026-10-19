using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SimplePauseMenuController : MonoBehaviour
{
    [SerializeField]
    private Animator _pauseMenuAnimator;

    private bool _isPauseMenuEnable = false;
    private bool _isPlaying = false;

    public async void Button_TogglePauseMenu()
    {
        if (_isPlaying) return;

        if (_isPauseMenuEnable){
            _pauseMenuAnimator.Play("PauseMenu_Disable");
        }
        else{
            _pauseMenuAnimator.Play("PauseMenu_Enable");
        }
        _isPauseMenuEnable = !_isPauseMenuEnable;

        _isPlaying = true;

        _pauseMenuAnimator.Update(0);
        float waitTime = _pauseMenuAnimator.GetCurrentAnimatorStateInfo(0).length;
        await Task.Delay((int)(waitTime * 1000));

        _isPlaying = false;
    }

    public void Button_RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Button_BackToMainMenuScene()
    {
        SceneManager.LoadScene(0);
    }
}
