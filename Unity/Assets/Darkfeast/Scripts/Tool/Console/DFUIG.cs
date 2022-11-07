using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DFUIG : MonoBehaviour
{
    GameObject ui_downTip;
    Text ui_Tip;
    Slider ui_Progress;
    Text ui_Speed;
    Button btn_clear;
    Button btn_ShowOrHide;
    ScrollRect ui_scrollRect;
    public bool isShow;

    private GameObject goScrollView;

    private Vector2 v2_downProgress = new Vector2(520, 54);
    private Vector2 v2_scrollView = new Vector2(820, Screen.height - 100 * 2);

    private Vector2 v2_txtDownSize = new Vector2(127, 34);
    private Vector3 v3_txtDownPos = new Vector3(-320,-1.7f,0);
    
    private void Awake()
    {
        InitUI();

        Application.logMessageReceived += ShowTips;
        DontDestroyOnLoad(this);
        DF.Log("DFUIG  init finish...");
    }

    void InitUI()
    {
        Sprite sp = Resources.Load<Sprite>("Textures/color");
        Sprite mask = Resources.Load<Sprite>("Textures/mask68");
        Font f = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject goCanvas = AddComs<Canvas,CanvasScaler,GraphicRaycaster>("Canvas",a=>a.renderMode=RenderMode.ScreenSpaceOverlay).gameObject;
        ScrollRect scrollRect=null;
        goScrollView = AddCom<ScrollRect>("Scroll View", t =>
        {
            t.horizontal = false;
            scrollRect = t;
        }).gameObject.Parent(goCanvas).gameObject;

        AddCom<Image>(goScrollView, t =>
        {
            t.sprite = sp;
            t.color = new Color(0.33f, 0.3f, 0.3f, 0.91f); 
        });

        RectTransform rectScrollView = SetRect(goScrollView, 0.5f, 0.5f, 0.5f, 1, 0.5f, 1);
        rectScrollView.sizeDelta = v2_scrollView;
        // DF.Log(rectScrollView.localPosition + "    " + rectScrollView.position + "   " + rectScrollView.anchoredPosition);
        //rectScrollView.localPosition = new Vector2(0, 160);   // Vector2.zero; PosY= Y - canvas.PosY  
        rectScrollView.anchoredPosition = new Vector2(0, -rectScrollView.sizeDelta.y / 2 - 100);

        GameObject goViewport = AddComs<Image, Mask>("Viewport", a => a.sprite = sp, b => b.showMaskGraphic = false).Parent(goScrollView).gameObject;
        scrollRect.viewport = SetRect(goViewport, 0.5f, 1, 0, 0, 1, 1, 0, 0, 0, 0);

        GameObject goContent = new GameObject("Content").Parent(goViewport).gameObject;
        scrollRect.content = SetRect(goContent, 0.5f, 1, 0, 0, 1, 1, 0, -900, 1, 2);
        ui_scrollRect = scrollRect;
       
        GameObject goTip = AddCom<Text>("Tip", t =>
        {
            t.text = "---------------------------------------------------------------------------------";
            t.font = f;
            t.fontSize = 30;
            ui_Tip = t;
        }).gameObject.Parent(goContent).gameObject;
        SetRect(goTip, 0.5f, 1f, 0, 0, 1, 1, 0, 0, 0, -v2_downProgress.y);

        GameObject goDownTip = new GameObject("downTip").Parent(goCanvas).gameObject;
        GameObject goDown = AddCom<Text>("down", t =>
        {
            t.font = f;
            t.fontSize = 26;
            t.text = "下载进度:";
            t.color = new Color(0.6f, 1, 0.1f);
            
        }).gameObject.Parent(goDownTip).gameObject;
        
        RectTransform rectDown = SetRect(goDown, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f);
        rectDown.sizeDelta = v2_txtDownSize;
        rectDown.localPosition = v3_txtDownPos;

        GameObject goSpeed = AddCom<Text>("speed", t =>
        {
            t.font = f;
            t.fontSize = 27;
            t.color = new Color(0, 1, 0.84f);
        }).gameObject.Parent(goDownTip).gameObject;
        
        RectTransform rectSpeed = SetRect(goSpeed, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f);
        rectSpeed.sizeDelta = new Vector2(150, 30);
        rectSpeed.localPosition = new Vector3(340, 0, 0);

        GameObject goDownProgress = new GameObject("downProgress").gameObject.Parent(goDownTip).gameObject;
        Slider sliderDownProgress = goDownProgress.AddComponent<Slider>();
        RectTransform rectDownProgress = goDownProgress.GetComponent<RectTransform>();
        rectDownProgress.sizeDelta = v2_downProgress;
        rectDownProgress.localPosition = Vector3.zero;
        goDownTip.transform.localPosition = new Vector3(0, (rectScrollView.sizeDelta.y -rectDownProgress.sizeDelta.y)/2f, 0);

        GameObject goBackGround = AddCom<Image>("background", t => t.sprite = sp).gameObject.Parent(goDownProgress).gameObject;
        SetRect(goBackGround, 0.5f, 0.5f, 0, 0.25f, 1, 0.75f, 0, 0, 0, 0);

        Image imgFill= null;
        GameObject goFill= AddCom<Image>("fill", t =>
        {
            imgFill = t;
            imgFill.sprite = sp;
            imgFill.color = new Color(0.02f, 1, 0.11f, 1);
        }).gameObject.Parent(goBackGround).gameObject;
        
        RectTransform rectFill = goFill.GetComponent<RectTransform>();
        sliderDownProgress.targetGraphic = imgFill;
        sliderDownProgress.fillRect = rectFill;
        SetRect(goFill, 0.5f, 0.5f, 0, 0, 1, 1); //必须位于 downProgressSlider.fillRect = rectFill;之后
        sliderDownProgress.value = 0.001f;
        ui_Progress = sliderDownProgress;
       
        
        AddComs<EventSystem,StandaloneInputModule,BaseInput>("EventSystem").Parent(goCanvas);;

        GameObject goBtnClear = AddCom<Image>("btnClose", t => t.color = DFHelper.ToolColor.GetColor("#858585")).transform.Parent(goCanvas.transform).gameObject;
        //SetRect(btnClear, 0.5f, 0.5f, 0.5f, 1, 0.5f,1);
        RectTransform rectBtnClear = SetRect(goBtnClear);
        rectBtnClear.sizeDelta = new Vector2(150, 60);
        rectBtnClear.localPosition = new Vector3(-220, -Screen.height / 2 + rectBtnClear.sizeDelta.y / 2 + 20, 0);
        AddCom<Button>(goBtnClear, t => t.onClick.AddListener(() => ui_Tip.text = ""));
        AddCom<Text>("txt_clear", t =>
        {
            t.font = f;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = "clear";
            t.fontSize = 35;

        }).gameObject.Parent(goBtnClear);

        
        GameObject goBtnShow = AddCom<Image>("btnShow", t =>t.color = DFHelper.ToolColor.GetColor("#858585")).gameObject.Parent(goCanvas).gameObject;
        RectTransform rectBtnShow = SetRect(goBtnShow);
        rectBtnShow.sizeDelta = new Vector2(150, 60);
        rectBtnShow.localPosition = new Vector3(220, -Screen.height / 2 + rectBtnShow.sizeDelta.y / 2 + 20, 0);
        AddCom<Button>(goBtnShow, t =>t.onClick.AddListener(() =>
        {
            goScrollView.State(!goScrollView.activeSelf);
            isShow = goScrollView.activeSelf;
        }));
        AddCom<Text>("txt_show", t =>
        {
            t.font = f;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = "show";
            t.fontSize = 35;

        }).gameObject.Parent(goBtnShow);
        
        
        ui_Speed = GameObject.Find("Canvas/downTip/speed").GetComponent<Text>();
        ui_downTip = goDownTip;
        ui_downTip.State(false);

        goScrollView.State(false);

        DontDestroyOnLoad(goCanvas);
    }

    RectTransform SetRect(GameObject go, float pivotX = 0.5f, float pivotY = 0.5f, float anchorMinX = 0.5f, float anchorMinY = 0.5f, float anchorMaxX = 0.5f, float anchorMaxY = 0.5f,
        float offsetMinX = 0, float offsetMinY = 0, float offsetMaxX = 0, float offsetMaxY = 0)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect == null)
            rect = go.AddComponent<RectTransform>();
        rect.pivot = new Vector2(pivotX, pivotY);
        rect.anchorMin = new Vector2(anchorMinX, anchorMinY);
        rect.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        rect.offsetMin = new Vector2(offsetMinX, offsetMinY);
        rect.offsetMax = new Vector2(offsetMaxX, offsetMaxY);
        return rect;
    }

    T AddCom<T>(string goName, Action<T> act) where T : Component
    {
        GameObject go = new GameObject(goName);
        T t= go.AddComponent<T>();
        act?.Invoke(t);
        return t;
    }
    
    T AddCom<T>(GameObject go, Action<T> act) where T : Component
    {
        T t= go.AddComponent<T>();
        act?.Invoke(t);
        return t;
    }

    GameObject AddComs<A, B>(string goName,Action<A> actA=null, Action<B> actB=null) where A : Component where B : Component
    {
        GameObject go = new GameObject(goName);
        A a = go.AddComponent<A>();
        B b = go.AddComponent<B>();
        actA?.Invoke(a);
        actB?.Invoke(b);
        return go;
    }
    
    GameObject AddComs<A, B, C>(string goName,Action<A> actA=null, Action<B> actB=null,Action<C> actC= null) where A : Component where B : Component where C : Component
    {
        GameObject go = new GameObject(goName);
        A a = go.AddComponent<A>();
        B b = go.AddComponent<B>();
        C c = go.AddComponent<C>();
        actA?.Invoke(a);
        actB?.Invoke(b);
        actC?.Invoke(c);
        return go;
    }

    void ShowTips(string msg, string stackTrace, LogType type)
    {
        // var vec2=new Vector2(0, ui_scrollRect.content.offsetMin.y - GetRow(msg) * 40);
        // ui_scrollRect.content.offsetMin = vec2;
        ui_Tip.text += msg + "\r\n";
        // ui_Tip.text += msg + "\n" + stackTrace + "\r\n";
        //tips = msg;
        //tips += "\r\n";
        Text t = null;

        //t.alignment = TextAnchor.MiddleCenter;
        //t.fontSize = 3;
        RectTransform rec = null;
        //rec.rect.siz
        //rec.sizeDelta
        //rec.pivot = new Vector2(0.5f, 0.5f);
        //rec.anchorMin = new Vector2(0, 0);
        //rec.anchorMax = new Vector2(0.5f, 0.5f);
        //rec.offsetMin =  Vector2.zero;
        //rec.offsetMax =  Vector2.zero;
    }

    int GetRow(string str)
    {
        Font font = ui_Tip.font;
        int row = 0;
        if (font != null)
        {
            font.RequestCharactersInTexture(str, ui_Tip.fontSize, FontStyle.Normal);
            CharacterInfo characterInfo;
            float width = 0f;
            for (int j = 0; j < str.Length; j++)
            {
                font.GetCharacterInfo(str[j], out characterInfo, ui_Tip.fontSize);
                width += characterInfo.advance;
            }

            row = (((int) (width / 580)) + 1);
            //DF.Log(str+ " row " + width  +"   "+row,E_ColorType.UI);
            return row;
        }
        return row;
    }

    public void Progress(float progress,string percent="1/1")
    {
        // DF.Log($"down  dependence {percentLeft}/{percentRight}", E_ColorType.UI);
        ui_Progress.value = progress;
    }

    public void ProgressSpeed(string speed = "100 k/s")
    {
        ui_Speed.text = speed;
    }

    public void ProgressState(bool state)
    {
        ui_downTip.State(state);
        if (state)
        {
            ui_Progress.value = 0;
        }
    }

    public void Show()
    {
        isShow = true;
        goScrollView.State(true);
    }


    private void OnDestroy()
    {
        Application.logMessageReceived -= ShowTips;
    }
}