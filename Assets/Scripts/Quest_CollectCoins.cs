using TMPro;
using UnityEngine;

public class Quest_CollectCoins : QuestParent
{
    public int coinsNeeded = 5;
    public GameObject FirstDialogue;
    public GameObject NotEnoughDialogue;
    public GameObject EnoughDialogue;
    public TextMeshProUGUI coinCounterText;

    PlayerLogic_Quest_CoinCollect playerCoinLogic;

    void Awake()
    {
        FirstDialogue.SetActive(true);
        NotEnoughDialogue.SetActive(false);
        EnoughDialogue.SetActive(false);
    }

    public override void StartQuest()
    {
        base.StartQuest();
        playerCoinLogic = GameManager.instance.GetComponentInPlayer<PlayerManager>().coinCollectMission;
        if (playerCoinLogic != null)
        {
            Destroy(FirstDialogue);
            playerCoinLogic.SetupMission(coinsNeeded);
        }
    }

    void FixedUpdate()
    {
        if(playerCoinLogic != null)
        {
            coinCounterText.text = playerCoinLogic.collectedCoins + " / " + coinsNeeded;

            if (playerCoinLogic.collectedCoins >= coinsNeeded)
            {
                EnoughDialogue.SetActive(true);
                NotEnoughDialogue.SetActive(false);
            }
            else
            {
                NotEnoughDialogue.SetActive(true);
                EnoughDialogue.SetActive(false);
            }
        }
    }

    public override void CompleteQuest()
    {
        base.CompleteQuest();
        if (playerCoinLogic != null)
        {
            playerCoinLogic.gameObject.SetActive(false);
            Destroy(FirstDialogue);
            Destroy(NotEnoughDialogue);
            Destroy(EnoughDialogue);
            Destroy(this.gameObject);
        }
    }
}
