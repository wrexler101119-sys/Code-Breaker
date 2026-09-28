using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoteButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text voteCountText;
    [SerializeField] private Button voteButton;

    private Player targetPlayer;

    public Player TargetPlayer => targetPlayer;

    public void Setup(Player player)
    {
        targetPlayer = player;

        playerNameText.text = player.Name;
        voteCountText.text = "0";

        voteButton.onClick.RemoveAllListeners();
        voteButton.onClick.AddListener(OnVotePressed);
    }

    public void SetVoteCount(int count)
    {
        voteCountText.text = count.ToString();
    }

    private void OnVotePressed()
    {
        if (MeetingManager.Instance != null)
        {
            MeetingManager.Instance.Vote(targetPlayer.Object);
        }
    }
}