using UnityEngine;
using TMPro;

public class EndGameHandler : MonoBehaviour
{
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private float endTimer = 3f;

    private float timer;
    private bool gameEnded;
    private int cubesInThreshold = 0;

    private void OnEnable() => CubeMergeSystem.OnMaxLevelReached += Victory;
    private void OnDisable() => CubeMergeSystem.OnMaxLevelReached -= Victory;

    public void Victory() => EndGame(true);

    private void Update()
    {
        if (gameEnded) return;

        if (cubesInThreshold > 0)
        {
            timer += Time.deltaTime;
            if (timer >= endTimer)
                EndGame(false);
        }
        else timer = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void EndGame(bool isVictory)
    {
        if (gameEnded) return;
        gameEnded = true;

        Time.timeScale = 0f;
        endGamePanel.SetActive(true);

        text.text = isVictory 
            ? $"You Win!\nScore: {ScoreHandler.Instance.Score}" 
            : $"Game Over!\nScore: {ScoreHandler.Instance.Score}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CubeMergeSystem cube))
        {
            if (!cube.IsNewBox)
                cubesInThreshold++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out CubeMergeSystem cube))
        {
            if (!cube.IsNewBox)
                cubesInThreshold--;
        }
        
        if (cubesInThreshold < 0) cubesInThreshold = 0;
    }

    private void OnTriggerStay(Collider other)
    {
        if (gameEnded) return;

        if (other.TryGetComponent(out CubeMergeSystem cube))
        {
            if (!cube.IsNewBox && timer == 0 && cubesInThreshold == 0)
                cubesInThreshold = 1; 
        }
    }
}