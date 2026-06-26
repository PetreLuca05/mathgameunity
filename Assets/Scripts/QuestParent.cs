using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events; // Add this

public class QuestParent : MonoBehaviour
{
    [Header("Quest Settings")]
    public GameObject questUI;
    public bool inQuestRange = false;
    public bool questCompleted = false;
    public bool questStarted = false;

    void Start()
    {
        Initialize();
    }

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (questUI != null)
            questUI.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!questStarted) return;
        if(!questUI) return;
        if(questCompleted == false)
        {
            questUI.SetActive(inQuestRange);
        } else {
            questUI.SetActive(false);
        }
    }

    public virtual void StartQuest()
    {
        Debug.Log("Quest Started: " + gameObject.name);
        questStarted = true;
        if (questUI != null)
            questUI.SetActive(true);
    }

    public virtual void CompleteQuest()
    {
        Debug.Log("Quest Completed: " + gameObject.name);
        questCompleted = true;
        if (questUI != null)
            Destroy(questUI);
        //Destroy(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            inQuestRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            inQuestRange = false;
        }
    }
}
