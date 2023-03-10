using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class ChallengeManager : MonoBehaviour
{
    public static ChallengeManager instance;
    public static Vector2 CalculateTotalPanelSize(Vector2 innerSize)
    {
        return innerSize + new Vector2(24 + 24, 24 + 46);
    }

    public static float GetDifferenceMod(float difference)
    {
        if (difference > 2) { return 0.7f; } else if (difference <= 2 && difference >= 1) { return Mathf.Lerp(0.7f, 0.9f, 2 - difference); } else if (difference <= 1 && difference >= 0) { return Mathf.Lerp(0.9f, 1f, 1 - difference); } else if (difference <= 0 && difference >= -1) { return Mathf.Lerp(1f, 1.2f, -difference); } else if (difference <= -1 && difference >= -2) { return Mathf.Lerp(1.2f, 1.5f, -1 - difference); } else if (difference <= -2 && difference >= -3) { return Mathf.Lerp(1.5f, 2f, -2 - difference); } else { return 2f - difference/3; }
    }

    public static float GetSanityMod(int sanity)
    {
        return sanity < 0 ? (1 - sanity / 25) : 1;
    }

   public static float GetRandomSpawnDelay(int day)
   {
      float delay = UnityEngine.Random.Range(1.5f, Mathf.Max(Mathf.Min(4f / Mathf.Pow(day, 0.35f) + (5f - day * 0.1f), 8), 4));
      Debug.Log($"Delay: {delay}");

        return delay;
    }

    public float dayTime = 40.0f;

    [SerializeField] GameObject challengeBasePrefab;
    [SerializeField] List<GameObject> challengesStrength;
    [SerializeField] List<GameObject> challengesDexterity;
    [SerializeField] List<GameObject> challengesIntelligence;
    [SerializeField] Transform challengeContainer;

    [SerializeField] RectTransform spawnAreaPanel;

    [SerializeField] Sprite strBorder;
    [SerializeField] Sprite dexBorder;
    [SerializeField] Sprite intBorder;

    [SerializeField] GameObject floatingTxt;

    private void Awake()
    {
        instance = this;
    }

    public void StartDay(int strength, int dexterity, int inteligence, int sanity, int day, Vector3 probabilities, Action onDayCompleted, Action onDayLost, bool tutorial = false)
    {
        Debug.LogWarning("Starting day");
        StartCoroutine(FinishDayAfterDusk(onDayCompleted, tutorial));
        StartCoroutine(ChallengeFactory(strength, dexterity, inteligence, sanity, day, probabilities, tutorial));
    }

    private IEnumerator FinishDayAfterDusk(Action onDayCompleted, bool tutorial)
    {
        if (tutorial == true)
        {
            yield return new WaitForSeconds(15f);
        }
        else
        {
            yield return new WaitForSeconds(dayTime);
        }

      SoundManager.Instance.playSound_clock();

        StopAllCoroutines();
        CleanupChallenges();
        onDayCompleted();
    }

    private void CleanupChallenges()
    {
        for (int i = challengeContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = challengeContainer.GetChild(i);

            if (child.name != "ChallengeSpawnArea")
            {
                Destroy(child.gameObject);
            }
        }
    }

    private IEnumerator ChallengeFactory(int strength, int dexterity, int inteligence, int sanity, int day, Vector3 probabilities, bool tutorial)
    {
        yield return new WaitForSeconds(3f);

        while (true)
        {
            SpawnTask(strength, dexterity, inteligence, sanity, day, probabilities, tutorial);

            if (tutorial == true)
            {
                yield return new WaitForSeconds(3600f);
            }
            else
            {
                yield return new WaitForSeconds(GetRandomSpawnDelay(day)); // Random time
            }
        }
    }

    void SpawnTask(int strength, int dexterity, int inteligence, int sanity, int day, Vector3 probabilities, bool tutorial)
    {
        SoundManager.Instance.playSound_taskSpawn();
        // Pick stat based on probabilities

        int stat = strength;
        int index = 0;

        float rand = UnityEngine.Random.Range(0f, 1f);
        if (rand - probabilities.x <= 0)
        {
            stat = strength;
            index = 0;
        }
        else if (rand - probabilities.x - probabilities.y <= 0)
        {
            stat = dexterity;
            index = 1;
        }
        else
        {
            stat = inteligence;
            index = 2;
        }

        List<GameObject> challengePrefabList = new List<GameObject>[] { challengesStrength, challengesDexterity, challengesIntelligence }[index];
        Sprite borderSprite = new Sprite[] { strBorder, dexBorder, intBorder }[index];

        if (challengePrefabList == null || challengePrefabList.Count == 0)
        {
            Debug.LogWarning("Tasklist is empty");
            return;
        }

        // Pick random challenge from list
        int randomIndex = UnityEngine.Random.Range(0, challengePrefabList.Count);
        GameObject randomChallenge = challengePrefabList[randomIndex];


        // TUTORIAL OVERRIDE
        if (tutorial == true)
        {
            stat = strength;
            index = 0;
            randomChallenge = challengesStrength[0]; // ButtonMash
        }

        // Spawn challenge and grab components
        GameObject challenge = Instantiate(randomChallenge);
        Transform challengePanel = challenge.transform.GetChild(0);

        RectTransform challengeRect = challengePanel.GetComponent<RectTransform>();


        // Spawn challengeBase and embed the selected challenge
        GameObject challengeBase = Instantiate(challengeBasePrefab, challengeContainer);
        Challenge challengeBaseChallengeComponent = challengeBase.GetComponent<Challenge>();
        Transform challengeBasePanel = challengeBase.transform.GetChild(0);
        RectTransform challengeBasePanelRect = challengeBasePanel.GetComponent<RectTransform>();
        Destroy(challenge, 0.1f);


        // Set the border
        challengeBasePanel.GetChild(0).GetComponent<Image>().sprite = borderSprite;

        ChallengeTrigger challengeObjectChallengeTrigger = challengePanel.GetComponent<ChallengeTrigger>();

      // Calculate and set difficulty mod
      float challengeMod = Mathf.Clamp(2 + Mathf.Round(UnityEngine.Random.Range(0, day * 0.25f)), 2, 5);
      float statDifference = (stat - challengeMod) * UnityEngine.Random.Range(0.8f, 1.2f);
      float differenceMod = GetDifferenceMod(statDifference);
      float sanityMod = GetSanityMod(sanity);
      float difficultyMod = differenceMod * sanityMod;
      challengeObjectChallengeTrigger.difficultyMod = difficultyMod;

        Debug.Log($"Spawned {randomChallenge.name} [ Sdiff {statDifference} -> {differenceMod} | sanity {sanityMod}, Tdiff {difficultyMod} ]");

        // Calculate and set challengeBase dimensions
        Vector2 challengeBaseSize = ChallengeManager.CalculateTotalPanelSize(challengeRect.sizeDelta);
        challengeBasePanel.GetComponent<RectTransform>().sizeDelta = challengeBaseSize;


        // Copy challenge prefabs to display
        Transform challengeBaseDisplay = challengeBase.transform.GetChild(0).GetChild(1);
        challengePanel.SetParent(challengeBaseDisplay);
        challengePanel.GetComponent<RectTransform>().localPosition = Vector3.zero;

        // Calculate spawn position
        Vector2 challengePanelSize = challengeBaseSize;


        // Get the available spawn range
        Vector2 maxSpawnRange = new Vector2(spawnAreaPanel.rect.width, spawnAreaPanel.rect.height);

        Vector2 spawnRange = maxSpawnRange - challengePanelSize;

        int randomX = (int)(UnityEngine.Random.Range(0, spawnRange.x) - spawnRange.x / 2);
        int randomY = (int)(UnityEngine.Random.Range(0, spawnRange.y) - spawnRange.y / 2);

        // Register events

        challengeObjectChallengeTrigger.OnTaskFulfilled += challengeBaseChallengeComponent.ChallengeFulfilled;
        challengeObjectChallengeTrigger.OnTaskFailure += challengeBaseChallengeComponent.ChallengeFailed;

        challengeBaseChallengeComponent.OnTaskFulfilled += () => { OnTaskFulfilled(differenceMod, new Vector2(randomX, randomY)); };
        challengeBaseChallengeComponent.OnTaskFailure += OnTaskFailure;

        challengeObjectChallengeTrigger.OnTimeTick += challengeBaseChallengeComponent.SetTimer;

        // Move the challenge panel
        challengeBase.transform.localPosition = new Vector3(randomX, randomY, challengeBase.transform.localPosition.z);
    }

    void OnTaskFulfilled(float difficultyMod, Vector2 position)
    {
        SoundManager.Instance.playSound_taskComplete();
        int money = Mathf.FloorToInt(100 * Mathf.Pow(difficultyMod, 5 / 3) * UnityEngine.Random.Range(0.9f, 1.1f) * Mathf.Pow(LoopManager.instance.currentDay, 0.9f));
        MoneyManager.instance.gainMoney(money);
        GameObject fTxt = Instantiate(floatingTxt, challengeContainer);
        fTxt.transform.localPosition = new Vector3(position.x, position.y, fTxt.transform.localPosition.z);
        fTxt.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = money.ToString() + "$";
        Debug.Log("Task fulfilled.");
    }

    void OnTaskFailure()
    {
        SoundManager.Instance.playSound_taskFailed();
        Debug.Log("Task failed successfully.");
    }
}
