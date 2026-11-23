using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Offers a scene transition only when the selected destination exists.</summary>
public sealed class StageEvolutionPortal : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject visual;
    [SerializeField] private string demonicScene = "ProfaneGardenStage02";
    [SerializeField] private float radius = 1.5f;
    private StageEvolutionLineage lineage;
    private bool visible;
    public void Configure(Transform playerTransform, GameObject portalVisual) { player = playerTransform; visual = portalVisual; if (visual != null) visual.SetActive(false); }
    public void Show(StageEvolutionLineage result) { lineage = result; visible = true; if (visual != null) visual.SetActive(true); }
    public bool IsPlayerInRange => visible && player != null &&
        Vector3.Distance(player.position, transform.position) <= radius;
    public bool DestinationAvailable => lineage == StageEvolutionLineage.Demonic &&
        Application.CanStreamedLevelBeLoaded(demonicScene);
    public bool TryAdvance()
    {
        if (!IsPlayerInRange || !DestinationAvailable) return false;
        SceneManager.LoadScene(demonicScene);
        return true;
    }
    private void OnGUI()
    {
        if (!IsPlayerInRange) return;
        Rect rect = new Rect(Screen.width - 256f, Screen.height - 204f, 220f, 72f);
        var style = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * .04f), 22, 36), fontStyle = FontStyle.Bold };
        if (GUI.Button(rect, DestinationAvailable ? "Próxima fase" : "Em breve", style))
            TryAdvance();
    }
}
