using UnityEngine;
using TMPro;
using UnityEngine.EventSystems; // UIのクリック検知に必要

public class RandomTextDisplay : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text textComponent;

    [TextArea(2, 5)] // ここを追加しました：Inspectorで複数行の入力と改行ができるようになります
    public string[] textList = new string[5] { "WAREWARE", "AREDARE", "TARE", "SARADA", "YARARETA" };

    void Start()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TMP_Text>();
        }

        SetRandomText();
    }

    // ランダムにテキストを更新するメソッド
    public void SetRandomText()
    {
        if (textList.Length > 0 && textComponent != null)
        {
            int randomIndex = Random.Range(0, textList.Length);
            textComponent.text = textList[randomIndex];
        }
    }

    // テキスト（UI）がクリックされたときに自動で呼ばれるメソッド
    public void OnPointerClick(PointerEventData eventData)
    {
        SetRandomText();
    }
}