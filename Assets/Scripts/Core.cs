using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Core : MonoBehaviour
{
    public string id = "";
    public string turnId = "";
    private string word = "";

    public List<Player> players = new List<Player>();
    public InputField joinField;
    public GameObject joinPanel;
    public InputField inputField;
    public ScrollRect scrollRect;
    public GameObject playersPanel;
    public Text wordText;
    public GameObject answerFound;
    public GameObject clearButton;
    public GameObject messagePrefab;
    public GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Join()
    {
        id = joinField.text;
        GetComponent<UConnect>().SendRequest("join", id);
        joinPanel.SetActive(false);
    }

    public void SendMsg()
    {
        if (id == turnId)
            return;

        if (inputField.text != "" && inputField.text != " ")
        {
            GetComponent<UConnect>().SendRequest("sendmsg", id, inputField.text);
            inputField.text = "";
        }
    }

    public void GetPlayers()
    {
        if (playersPanel.activeSelf)
        {
            playersPanel.SetActive(false);
        }
        else
        {
            GetComponent<UConnect>().SendRequest("getplayers");
            playersPanel.SetActive(true);
        }
    }

    public void ShowPlayers(NetworkData data)
    {
        ClearContent(playersPanel.transform.GetChild(0).GetComponent<ScrollRect>());

        foreach (string p in data.array)
        {
            GameObject msg = Instantiate(playerPrefab);
            msg.transform.SetParent(playersPanel.transform.GetChild(0).GetComponent<ScrollRect>().content);
            msg.transform.localPosition = Vector3.zero;
            msg.transform.localRotation = Quaternion.identity;
            msg.transform.localScale = Vector3.one;

            msg.GetComponent<Text>().text = p;
        }
    }

    public void ClearAllLines()
    {
        GetComponent<UConnect>().SendRequest("send", "clear");
    }

    public void ReceiveMsg(NetworkData data)
    {
        GameObject msg = Instantiate(messagePrefab);
        msg.transform.SetParent(scrollRect.content);
        msg.transform.localPosition = Vector3.zero;
        msg.transform.localRotation = Quaternion.identity;
        msg.transform.localScale = Vector3.one;

        msg.transform.GetChild(0).GetComponent<Text>().text = data.array[0];
        msg.transform.GetChild(1).GetComponent<Text>().text = data.array[1];

        StartCoroutine(DelayedScroll());
    }

    IEnumerator DelayedScroll()
    {
        yield return new WaitForSeconds(0.25f);
        scrollRect.verticalNormalizedPosition = 0;
    }

    public void LetsGo(NetworkData data)
    {
        GetComponent<Drawer>().ClearLines();
        answerFound.SetActive(false);

        turnId = data.array[0];
        word = data.array[1];

        wordText.text = (id == turnId) ? "Bunu Çiz: " + word : "Çizen: " + turnId;
        GetComponent<Drawer>().canDraw = (id == turnId);
        clearButton.SetActive(GetComponent<Drawer>().canDraw);
    }

    public void WordFound(NetworkData data)
    {
        StartCoroutine(WordFoundAsync(data));
    }

    IEnumerator WordFoundAsync(NetworkData data)
    {
        GetComponent<Drawer>().ClearLines();

        ReceiveMsg(data);
        answerFound.transform.GetChild(0).GetComponent<Text>().text = data.array[0];
        answerFound.transform.GetChild(1).GetComponent<Text>().text = data.array[1];
        answerFound.SetActive(true);

        yield return new WaitForSeconds(3);

        answerFound.SetActive(false);

        turnId = data.array[2];
        word = data.array[3];

        wordText.text = (id == turnId) ? "Bunu Çiz: " + word : "Çizen: " + turnId;
        GetComponent<Drawer>().canDraw = (id == turnId);
        clearButton.SetActive(GetComponent<Drawer>().canDraw);
        GetComponent<Drawer>().ClearLines();
    }

    public void ClearContent(UnityEngine.UI.ScrollRect scrollrect)
    {
        int count = scrollrect.content.childCount;

        for (int i = 0; i < count; i++)
        {
            Destroy(scrollrect.content.GetChild(i).gameObject);
        }
    }
}

public class Player
{
    public string id = "";
    public int point = 0;
}