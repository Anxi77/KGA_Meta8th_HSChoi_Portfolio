using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Linq;
using System;
using System.Globalization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
[Serializable]
public class SPUM_Animator{
    public string Type;
    public RuntimeAnimatorController RuntimeAnimator;
}
public class SPUM_Manager : MonoBehaviour
{
    public float _version;
    public string unitPath = "Assets/SPUM/Resources/Units/"; // 프리펩 저장 장소
    public string unitBackUpPath = "Assets/SPUM/Backup/";
    public bool isSaveSamePath = false;
    public SPUM_Prefabs EditPrefab;
    public SPUM_Prefabs PreviewPrefab; // 프리뷰 프리펩
    public SPUM_Animator[] SPUM_Animator; 
    public Dictionary<string, RuntimeAnimatorController> SPUM_AnimatorDic = new();
    public Toggle RandomColorButton;
    //public List<SPUM_PackageButton> _packageButtonList = new List<SPUM_PackageButton>(); // 스펌 생성된 패키지 버튼 리스트
    //public Dictionary<string, bool> SpritePackagesFilterList = new Dictionary<string, bool>(); //보여질 패키지 상태 관리
    public List<SpumPackage> spumPackages;
    public List<string> StateList = new ();
    public List<string> UnitTypeList = new();
    public List<string> SpritePackageNameList = new();
    [Header("Manager")]
    [SerializeField] public SPUM_AnimationManager animationManager;
    [SerializeField] public SPUM_UIManager UIManager;
    [SerializeField] public SPUM_PaginationManager paginationManager;

#if UNITY_EDITOR
#region Unity Function
    void Awake()
    {
        SoonsoonData.Instance._spumManager= this;
        LoadPackages();
        var unit = PreviewPrefab;
        if(unit.spumPackages.Count.Equals(0)) {
            var InitLegacyData = GetSpumLegacyData();
            if(string.IsNullOrEmpty(unit.UnitType)) unit.UnitType = "Unit";
            unit.spumPackages = InitLegacyData;
        }
        var uniqueTypes = spumPackages
            .SelectMany(package => package.SpumAnimationData)
            .Select(clip => clip.StateType.ToUpper())
            .Distinct(System.StringComparer.OrdinalIgnoreCase)
            .ToList();
        StateList = uniqueTypes;

        var uniqueUnitTypes = spumPackages
            .SelectMany(package => package.SpumAnimationData)
            .Select(clip => clip.UnitType)
            .Distinct(System.StringComparer.OrdinalIgnoreCase)
            .ToList();
        UnitTypeList = uniqueUnitTypes;

        var uniquePackageNames = spumPackages
        .Where(package => package.SpumTextureData.Count > 0)
        .Select(package => package.Name)
        .Distinct(System.StringComparer.OrdinalIgnoreCase)
        .ToList();
        SpritePackageNameList = uniquePackageNames;
    }
    void Start()
    {
        


        SPUM_AnimatorDic = SPUM_Animator.ToDictionary(item => item.Type, item => item.RuntimeAnimator);
        
        StartCoroutine(StartProcess());
        SetType("Unit");
        ItemResetAll();
    }
    [ContextMenu("Setup")]
    public void Setup(){
        SetDefultSet("Unit", "Body", "Human_1", Color.white);
        SetDefultSet("Unit", "Eye", "Eye0", new Color32(71, 26,26, 255));
        SetDefultSet("Horse", "Body", "Horse1", Color.white);
    }
#endregion


    // public void SetPackageActiveStateList(){{
    //     SpritePackagesFilterList = animationManager.SpritePackageNameList.ToDictionary(name => name, name => true);
    // }}
    // 타입을 설정 유닛 / 말
    public IEnumerator StartProcess()
    {
        Debug.Log("Data Load processing..");

        // 버전 체크 및 패키지 데이터 체크
        yield return StartCoroutine(SoonsoonData.Instance.LoadData());
        

        // bool dirChk = Directory.Exists("Assets/Resources/SPUM/SPUM_Sprites/Items");
        // if(!dirChk)
        //     UIManager.OnNotice("[Empty body image source]\n\nYou need setup first\nPlease Sprite images locate to Resource Folder\nPlease Read Readme.txt file",1,1);
        //     yield return null;

        //     //yield return StartCoroutine(GetPrefabList());
        //     //프리팹 연동
        //     ShowNowUnitNumber(); //프리팹 숫자 연동

        //     SetInit();
        //     //기본 색 연동
        //     //UI연동.


        // 작업 색상 정보 로드
        if( SoonsoonData.Instance._soonData2._savedColorList == null ||  SoonsoonData.Instance._soonData2._savedColorList.Count.Equals(0))
        {
            SoonsoonData.Instance._soonData2._savedColorList = new List<string>();
            for(var i = 0 ; i < 17 ;i++)
            {
                SoonsoonData.Instance._soonData2._savedColorList.Add("");
            }
            SoonsoonData.Instance.SaveData();
        }
        else
        {
            //Debug.Log( SoonsoonData.Instance._soonData2._savedColorList.Count);
            for(var i = 0 ; i < SoonsoonData.Instance._soonData2._savedColorList.Count ;i++)
            {
                string tSTR = SoonsoonData.Instance._soonData2._savedColorList[i];
                //Debug.Log(tSTR);
                Color parsedColor;
                if(ColorUtility.TryParseHtmlString(tSTR, out parsedColor)){
                    UIManager._colorSaveList[i]._savedColor.gameObject.SetActive(true);
                    UIManager._colorSaveList[i]._savedColor.color = parsedColor;
                }else{
                    if(string.IsNullOrWhiteSpace(tSTR)) continue;
                    Debug.LogWarning(tSTR + " is Invalid color information");
                }
            }

        }
    }
    private void LoadPackages()
    {
        spumPackages.Clear();
        var jsonFileArray = Resources.LoadAll<TextAsset>("");
        //Debug.Log("Index" + jsonFileArray.Length);
        foreach (var asset in jsonFileArray)
        {
            if(!asset.name.Contains("Index")) continue;
            if(!asset) continue;
            Debug.Log(asset);
            var Package = JsonUtility.FromJson<SpumPackage>(asset.ToString());
            spumPackages.Add(Package);
        }
        var dataSortList = spumPackages.OrderBy(p => DateTime.ParseExact(p.CreationDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).ToList();
        spumPackages = dataSortList;
    }
    public List<SpumPackage> GetSpumPackageData(){
        return spumPackages;
    }
    public List<SpumPackage> GetSpumLegacyData(){
        string targetString = "Legacy";
        var LegacyData = spumPackages;
        foreach (var package in LegacyData)
        {
            if(package.Name.Equals(targetString)) {
                foreach (var data in package.SpumAnimationData)
                {
                    data.HasData = true;
                }
            }
        }
        return LegacyData; // Package;
    }
#region PreviewItemPanel
    public void DrawItemList(SPUM_SpriteButtonST ButtonData)
    {
        UIManager.SetPackageButtons(ButtonData);
        
        var enabledPackages =  UIManager.SpritePackagesFilterList
            .Where(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();
        
        UIManager._panelTitle.text = ButtonData.PartType;

        UIManager.ClearPreviewItems();

        var SpumPackages =  spumPackages;

        string unitType = ButtonData.UnitType;
        string partType = ButtonData.PartType;

        //패키지 구룹화 - 패키지 필터 조건
        var groupedPackageData = SpumPackages
            .Where(p => enabledPackages.Contains(p.Name)) // 패키지 이름이 enabledPackages 리스트에 있는 것만 선택
            .SelectMany(p => p.SpumTextureData.Select(t => new { Package = p, Texture = t }))
            .Where(x => x.Texture.PartType.Equals(partType) && x.Texture.UnitType.Equals(unitType))
            .GroupBy(x => new { x.Texture.PartType, x.Texture.Name, x.Texture.UnitType }) // 구룹 키
            .Select(g => new 
            { 
                Key = g.Key, 
                Items = g.ToList() 
            })
            .ToList();
        
        // 필터된 패키지 아이템 순환
        foreach (var package in groupedPackageData) 
        {
            
            // 프리뷰 아이템 버튼 생성
            var PreviewItem = UIManager.CreatePreviewItem();
            var previewButton = PreviewItem.GetComponent<SPUM_PreviewItem>();
            previewButton.name = package.Key.Name;
            // Hide Other Element
            foreach (Transform tr in previewButton.transform)
            {
                tr.gameObject.SetActive(tr.name.ToUpper() == ButtonData.ItemShowType.ToUpper());
            }
            previewButton.SetSpriteButton.onClick.AddListener(()=> { ButtonData.IsActive = true; } );

            //Preview Element Matching Map
            var UnitPartLists = previewButton.GetComponentsInChildren<SPUM_PreviewMatchingList>(); 

            // 보여질 이미지 추출
            var UnitPartList = UnitPartLists 
                .SelectMany(table => table.matchingTables)
                .Where(element => element.PartType == package.Key.PartType && element.UnitType == package.Key.UnitType)
                .ToList();
            //Debug.Log($"PartType: {group.Key.PartType}, Name: {group.Key.Name}, UnitType: {group.Key.UnitType}");
            foreach (var item in package.Items) // 아이템 개별 항목 순환
            {
                //Debug.Log($"  Package: {item.Package.Name}, SubType: {item.Texture.SubType}, Path: {item.Texture.Path}");
                foreach (var part in UnitPartList)
                {
                    if(part.Structure == item.Texture.SubType){ // 멀티플 타입 이미지 인 경우
                        var LoadSprite = LoadSpriteFromMultiple(item.Texture.Path, item.Texture.SubType);
                        part.ItemPath = item.Texture.Path;
                        part.image.sprite = LoadSprite;
                        part.PartSubType = item.Texture.PartSubType;
                        float pixelWidth = LoadSprite.rect.width;
                        float pixelHeight = LoadSprite.rect.height;

                        float unityWidth = pixelWidth / LoadSprite.pixelsPerUnit;
                        float unityHeight = pixelHeight / LoadSprite.pixelsPerUnit;

                        part.image.rectTransform.sizeDelta = new Vector2(unityWidth * 100, unityHeight * 100);
                        part.Dir = ButtonData.Direction;
                        Color PartColor = ButtonData.ignoreColorPart.Contains(item.Texture.SubType) ? Color.white : ButtonData.PartSpriteColor;
                        part.image.color = PartColor;
                        part.Color = PartColor;
                        part.MaskIndex = (int)ButtonData.SpriteMask;
                        previewButton.ImageElement.Add(part);
                    }
                    else if(UnitPartList.Count == 1) // 서브타입이 없는 멀티플이 아닌경우 
                    {
                        var LoadSprite = LoadSpriteFromMultiple(item.Texture.Path, item.Texture.SubType);
                        part.ItemPath = item.Texture.Path;
                        part.image.sprite = LoadSprite;
                        part.PartSubType = item.Texture.PartSubType;
                        float pixelWidth = LoadSprite.rect.width;
                        float pixelHeight = LoadSprite.rect.height;

                        float unityWidth = pixelWidth / LoadSprite.pixelsPerUnit;
                        float unityHeight = pixelHeight / LoadSprite.pixelsPerUnit;

                        part.image.rectTransform.sizeDelta = new Vector2(unityWidth * 100, unityHeight * 100);
                        part.Dir = ButtonData.Direction;
                        part.Structure = item.Texture.SubType.Equals(item.Texture.Name) ? package.Key.PartType : item.Texture.SubType;
                        Color PartColor = ButtonData.ignoreColorPart.Contains(item.Texture.PartSubType) ? Color.white : ButtonData.PartSpriteColor;
                        part.image.color = PartColor;
                        part.Color = PartColor;
                        part.MaskIndex = (int)ButtonData.SpriteMask;
                        previewButton.ImageElement.Add(part);
                    }
                }
            }
            previewButton.ImageElement = previewButton.ImageElement.Distinct().ToList();
            // 필요없는 항목 비활성화
            UnitPartList.ForEach(item => 
            {
                item.image.gameObject.SetActive(item.image.sprite != null);
            });

            // 바뀔 프리뷰 항목 가져오기
        }
        // 프리뷰 아이템 데이터를 적용 필요
        UIManager.ShowItem();
    }
#endregion

#region PreviewUnit
    public void SetType(string Type){
        var PreviewUnit = PreviewPrefab;
        PreviewUnit.UnitType = Type;
        foreach (Transform child in PreviewUnit.transform)
        {
            child.gameObject.SetActive(child.name.Contains(Type));
        }
        var anim = PreviewUnit.GetComponentInChildren<Animator>();
        PreviewPrefab._anim = anim;

        if(Type.Equals("Unit"))
        {
            var ElementList = PreviewUnit.ImageElement;
            ElementList.RemoveAll(element => element.UnitType != Type);
        }else{
            SetDefultSet(Type, "Body", Type+"1", Color.white);
        }
        
    }
    public void SetDefultSet(string UnitType, string PartType, string TextureName, Color color)
    {
        string PackageName ="Legacy";
        //패키지 구룹화
        var groupedData = spumPackages
            .SelectMany(p => p.SpumTextureData.Select(t => new { Package = p, Texture = t }))
            .Where(x => x.Texture.PartType.Equals(PartType) && x.Texture.UnitType.Equals(UnitType) && x.Package.Name.Equals(PackageName) && x.Texture.Name.Equals(TextureName))
            .GroupBy(x => new { x.Texture.PartType, x.Texture.Name, x.Texture.UnitType }) // 그룹 키
            .Select(g => new 
            { 
                Key = g.Key, 
                Items = g.ToList() 
            })
            .ToList();

        var Parts = groupedData[0];

        var ListElement = new List<PreviewMatchingElement>();
        foreach (var item in Parts.Items)
        {
            //Debug.Log($"Path: {PartType}, SubType: { item.Texture.SubType}");
            var part = new PreviewMatchingElement();
            part.UnitType = UnitType;
            part.PartType = PartType;
            part.PartSubType = item.Texture.PartSubType;
            part.Dir = "";
            part.ItemPath = item.Texture.Path;
            part.Structure = item.Texture.SubType.Equals(item.Texture.Name) ? PartType : item.Texture.SubType;
            part.MaskIndex = 0;
            part.Color = color;

            ListElement.Add(part);
        }
        SetSprite(ListElement);
    }
    public void SetSprite(List<PreviewMatchingElement> ImageElement)
    {
        var PreviewUnit = PreviewPrefab;
        SaveElementData(ImageElement);

        // 프리뷰 유닛의 매칭 리스트를 가지고 온다.
        var matchingTables = PreviewUnit.GetComponentsInChildren<SPUM_MatchingList>(true);

        var allMatchingElements = matchingTables.SelectMany(mt => mt.matchingTables);
        //Debug.Log(ImageElement.Count + "SetSpriteCount" + allMatchingElements.Count());
        foreach (var matchingElement in allMatchingElements)
        {
            var matchingTypeElement = ImageElement.FirstOrDefault(ie => 
            (ie.UnitType == matchingElement.UnitType)
            && ("Weapons" == matchingElement.PartType)
            //&& ie.Index == matchingElement.Index
            && (ie.Dir == matchingElement.Dir) 
            && (ie.Structure == matchingElement.Structure)
            );
            //Debug.Log(matchingTypeElement != null);
            if (matchingTypeElement != null)
            {
                matchingElement.renderer.sprite = null;
                matchingElement.renderer.maskInteraction = (SpriteMaskInteraction) matchingTypeElement.MaskIndex;
                matchingElement.renderer.color = matchingTypeElement.Color;
                matchingElement.ItemPath = "";
                matchingElement.Color = matchingTypeElement.Color;
            }
        }
        #if UNITY_2023_1_OR_NEWER
            var ItemButtons = FindObjectsByType<SPUM_SpriteButtonST>(FindObjectsSortMode.None);
        #else
            #pragma warning disable CS0618
            var ItemButtons = FindObjectsOfType<SPUM_SpriteButtonST>();
            #pragma warning restore CS0618
        #endif

        foreach (var matchingElement in allMatchingElements)
        {
            var matchingTypeElement = ImageElement.FirstOrDefault(ie => 
            (ie.UnitType == matchingElement.UnitType)
            && (ie.PartType == matchingElement.PartType)
            //&& ie.Index == matchingElement.Index
            && (ie.Dir == matchingElement.Dir) 
            && (ie.Structure == matchingElement.Structure) 
            && (ie.PartSubType == matchingElement.PartSubType)
            //&& !string.IsNullOrEmpty( matchingElement.PartSubType )
            );
            //Debug.Log(matchingTypeElement != null);
            if (matchingTypeElement != null)
            {
                var existingElement = ItemButtons.FirstOrDefault(e => e.PartType == matchingTypeElement.PartType);
                var LoadSprite = LoadSpriteFromMultiple(matchingTypeElement.ItemPath , matchingTypeElement.Structure);
                matchingElement.renderer.sprite = LoadSprite;
                matchingElement.renderer.maskInteraction = (SpriteMaskInteraction) matchingTypeElement.MaskIndex;
                Color PartColor = existingElement.ignoreColorPart.Contains(matchingTypeElement.PartType) ? Color.white : matchingTypeElement.Color;
                matchingElement.renderer.color = PartColor;
                matchingElement.ItemPath = matchingTypeElement.ItemPath;
                matchingElement.Color =  PartColor;
            }
        }
        UIManager.DrawItemOff();
    }

    public void SetPartRandom(SPUM_SpriteButtonST ButtonData)
    {
        var isSpriteFixed = ButtonData.IsSpriteFixed;
        var UnitType = ButtonData.UnitType;
        var PartType = ButtonData.PartType;

        if(isSpriteFixed) return;
        string unitType = UnitType;
        string partType = PartType;

        //패키지 구룹화
        var groupedData = spumPackages
            .SelectMany(p => p.SpumTextureData.Select(t => new { Package = p, Texture = t }))
            .Where(x => x.Texture.PartType.Equals(partType) && x.Texture.UnitType.Equals(unitType))
            .GroupBy(x => new { x.Texture.PartType, x.Texture.Name, x.Texture.UnitType }) // 구룹 키
            .Select(g => new 
            { 
                Key = g.Key, 
                Items = g.ToList() 
            })
            .ToList();
        int randomValue = UnityEngine.Random.Range(0, groupedData.Count+1);

        if(randomValue.Equals(groupedData.Count)) 
        {
            ButtonData.RemoveSprite();
            return;
        }
        var randomGroup = groupedData[randomValue];

        var ListElement = new List<PreviewMatchingElement>();
        foreach (var item in randomGroup.Items)
        {
            //Debug.Log($"Path: {PartType}, SubType: { item.Texture.SubType}");
            var part = new PreviewMatchingElement();
            part.UnitType = UnitType;
            part.PartType = PartType;
            part.PartSubType = item.Texture.PartSubType;
            part.Dir = ButtonData.Direction;
            part.ItemPath = item.Texture.Path;
            part.Structure = item.Texture.SubType.Equals(item.Texture.Name) ? PartType : item.Texture.SubType;
            part.MaskIndex = (int)ButtonData.SpriteMask;
            Color PartColor = ButtonData.ignoreColorPart.Contains(item.Texture.SubType) ? Color.white : ButtonData.InitColor;
            part.Color = PartColor;

            ListElement.Add(part);
        }
        SetSprite(ListElement);
    }
    public void SetDefaultPart(SPUM_SpriteButtonST ButtonData)
    {
        var isSpriteFixed = ButtonData.IsSpriteFixed;
        var IsActive = ButtonData.IsActive;
        var UnitType = ButtonData.UnitType;
        var PartType = ButtonData.PartType;
        var PackageName = ButtonData.DefaultPackageName;
        var TextureName = ButtonData.DefaultTextureName;
        if(isSpriteFixed) return;
        IsActive = true;
        ButtonData.PartSpriteColor = ButtonData.InitColor;
        //Debug.Log(ButtonData.PartType + " " +  ButtonData.InitColor);
        //패키지 구룹화
        var groupedData = spumPackages
            .SelectMany(p => p.SpumTextureData.Select(t => new { Package = p, Texture = t }))
            .Where(x => x.Texture.PartType.Equals(PartType) && x.Texture.UnitType.Equals(UnitType) && x.Package.Name.Equals(PackageName) && x.Texture.Name.Equals(TextureName))
            .GroupBy(x => new { x.Texture.PartType, x.Texture.Name, x.Texture.UnitType }) // 그룹 키
            .Select(g => new 
            { 
                Key = g.Key, 
                Items = g.ToList() 
            })
            .ToList();
        //Debug.Log($"UnitType: {UnitType}, PartType: { PartType}, PackageName: { PackageName}, TextureName: { TextureName} / {groupedData.Count}");
        var randomGroup = groupedData[0];
        var ListElement = new List<PreviewMatchingElement>();
        foreach (var item in randomGroup.Items)
        {
            //Debug.Log($"Path: {PartType}, SubType: { item.Texture.Name}");
            var part = new PreviewMatchingElement();
            part.UnitType = UnitType;
            part.PartType = PartType;
            part.PartSubType = item.Texture.PartSubType;
            part.Dir = ButtonData.Direction;
            part.ItemPath = item.Texture.Path;
            part.Structure = item.Texture.SubType.Equals(item.Texture.Name) ? PartType : item.Texture.SubType;  
            part.MaskIndex = 0;
            //part.Color = ButtonData.InitColor;
            
            Color PartColor = ButtonData.ignoreColorPart.Contains(item.Texture.SubType) ? Color.white : ButtonData.InitColor;
            part.Color = PartColor;

            ListElement.Add(part);
        }
        SetSprite(ListElement);
    }
    public void RemoveSprite(SPUM_SpriteButtonST ButtonData)
    {
        var PreviewUnit = PreviewPrefab;
        var isSpriteFixed = ButtonData.IsSpriteFixed;
        var IsActive = ButtonData.IsActive;
        var UnitType = ButtonData.UnitType;
        var PartType = ButtonData.PartType;

        if(isSpriteFixed) return;
        ButtonData.IsActive = false;
        ButtonData.PartSpriteColor = ButtonData.InitColor;
        var matchingTables = PreviewUnit.GetComponentsInChildren<SPUM_MatchingList>(true);

        var allMatchingElements = matchingTables.SelectMany(mt => mt.matchingTables)
            .Where(element => 
                element.UnitType == UnitType &&
                element.PartType == PartType &&
                element.Dir == ButtonData.Direction
            );
        var ListElement = new List<PreviewMatchingElement>();
        foreach (var matchingElement in allMatchingElements)
        {
            //Debug.Log($"{matchingElement.UnitType} {matchingElement.Structure} {matchingElement.Dir}");
            var part = new PreviewMatchingElement();
            part.UnitType = UnitType;
            part.PartType = PartType;
            part.PartSubType = matchingElement.PartSubType;
            part.Dir = matchingElement.Dir;
            part.Structure = matchingElement.Structure;

            matchingElement.renderer.sprite = null;
            matchingElement.renderer.color = ButtonData.InitColor;
            matchingElement.ItemPath = "";
            matchingElement.Color = ButtonData.InitColor;
            ListElement.Add(part);
        }
        RemoveElementData(ListElement);
    }

    public void SetSpriteColor(SPUM_SpriteButtonST ButtonData)
    {
        var PreviewUnit = PreviewPrefab;
        var isSpriteFixed = ButtonData.IsSpriteFixed;
        var IsActive = ButtonData.IsActive;
        var UnitType = ButtonData.UnitType;
        var PartType = ButtonData.PartType;

        if(isSpriteFixed) return;
        var matchingTables = PreviewUnit.GetComponentsInChildren<SPUM_MatchingList>(true);

        var allMatchingElements = matchingTables.SelectMany(mt => mt.matchingTables)
            .Where(element => 
                element.UnitType == UnitType &&
                element.PartType == PartType &&
                element.Dir == ButtonData.Direction
            );
        var ListElement = new List<PreviewMatchingElement>();
        foreach (var matchingElement in allMatchingElements)
        {
            //Debug.Log($"{matchingElement.UnitType} {matchingElement.Structure} {matchingElement.Dir}");
            var part = new PreviewMatchingElement();
            part.UnitType = UnitType;
            part.PartType = PartType;
            part.Dir = matchingElement.Dir;
            part.Structure = matchingElement.Structure;  
            Color PartColor = ButtonData.ignoreColorPart.Contains(matchingElement.Structure) ? Color.white : ButtonData.PartSpriteColor;
            part.Color = PartColor;
            matchingElement.Color = PartColor;
            matchingElement.renderer.color = PartColor;
            ListElement.Add(part);
        }
        SetElementColorData(ListElement);
    }
    public void SetSpriteVisualMaskIndex(SPUM_SpriteButtonST ButtonData)
    {
        var PreviewUnit = PreviewPrefab;
        var isSpriteFixed = ButtonData.IsSpriteFixed;
        var IsActive = ButtonData.IsActive;
        var UnitType = ButtonData.UnitType;
        var PartType = ButtonData.PartType;

        //if(isSpriteFixed) return;
        var matchingTables = PreviewUnit.GetComponentsInChildren<SPUM_MatchingList>(true);

        var allMatchingElements = matchingTables.SelectMany(mt => mt.matchingTables)
            .Where(element => 
                element.UnitType == UnitType &&
                element.PartType == PartType &&
                element.Dir == ButtonData.Direction
            );
        var ListElement = new List<PreviewMatchingElement>();
        foreach (var matchingElement in allMatchingElements)
        {
            matchingElement.renderer.maskInteraction = ButtonData.SpriteMask;
            var part = new PreviewMatchingElement();
            part.UnitType = UnitType;
            part.PartType = PartType;
            part.Dir = matchingElement.Dir;
            part.Structure = matchingElement.Structure;  
            part.MaskIndex = (int)ButtonData.SpriteMask;
            ListElement.Add(part);
        }
        SetElementMaskData(ListElement);
    }
    public void ItemRandomAll()
    {
        #if UNITY_2023_1_OR_NEWER
            var ItemButtons = FindObjectsByType<SPUM_SpriteButtonST>(FindObjectsSortMode.None);
        #else
            #pragma warning disable CS0618
            var ItemButtons = FindObjectsOfType<SPUM_SpriteButtonST>();
            #pragma warning restore CS0618
        #endif
        List<string> conditionTypes = new List<string> {"Body", "Horse" };

        var filteredButtons = ItemButtons.Where(button => !conditionTypes.Contains(button.ItemShowType)).ToList();
        foreach (var button in filteredButtons)
        {
            button.SetPartRandom();
        }
    }
    public void ItemResetAll()
    {
        //Debug.Log("ItemResetAll");
        #if UNITY_2023_1_OR_NEWER
            var ItemButtons = FindObjectsByType<SPUM_SpriteButtonST>(FindObjectsSortMode.None);
        #else
            #pragma warning disable CS0618
            var ItemButtons = FindObjectsOfType<SPUM_SpriteButtonST>();
            #pragma warning restore CS0618
        #endif
        foreach (var button in ItemButtons)
        {
            button.RemoveSprite();
        }
        
        ResetBody();
    }
    public void ResetBody()
    {
        SetType(PreviewPrefab.UnitType);

        #if UNITY_2023_1_OR_NEWER
            var ItemButtons = FindObjectsByType<SPUM_SpriteButtonST>(FindObjectsSortMode.None);
        #else
            #pragma warning disable CS0618
            var ItemButtons = FindObjectsOfType<SPUM_SpriteButtonST>();
            #pragma warning restore CS0618
        #endif
        List<string> conditionTypes = new List<string> { "Eye" , "Body" };
        if(PreviewPrefab.UnitType.Equals("Horse")){
            conditionTypes.Add("Horse");
        }
        var filteredButtons = ItemButtons.Where(button => conditionTypes.Contains(button.ItemShowType)).ToList();
        foreach (var button in filteredButtons)
        {
            //Debug.Log(button.ItemShowType);
            button.SetInitPart();
        }
    }

    public void ItemLoadButtonActive(List<PreviewMatchingElement> ImageElement)
    {
        string[] uniquePartTypes = ImageElement.Select(m => m.PartType).Distinct().ToArray();
        var partTypeUnitTypeColorDict = ImageElement
                                            .GroupBy(m => new { m.PartType, m.UnitType, m.Dir })
                                            .ToDictionary(g => g.Key, g => g.First().Color);
        #if UNITY_2023_1_OR_NEWER
            var ItemButtons = FindObjectsByType<SPUM_SpriteButtonST>(FindObjectsSortMode.None);
        #else
            #pragma warning disable CS0618
            var ItemButtons = FindObjectsOfType<SPUM_SpriteButtonST>();
            #pragma warning restore CS0618
        #endif

        foreach (var button in ItemButtons)
        {
            var key = new { PartType = button.ItemShowType, UnitType = button.UnitType, Dir = button.Direction };
            
            if (partTypeUnitTypeColorDict.ContainsKey(key))
            {
                button.IsActive = true;
                button.PartSpriteColor = partTypeUnitTypeColorDict[key];
            }
            else
            {
                button.IsActive = false;
            }
        }
    }
    
    public void SetPrefabToPreviewPackageData(List<SpumPackage> packages){
        if(packages.Count.Equals(0)){
            PreviewPrefab.spumPackages = GetSpumLegacyData();
        }else{
            PreviewPrefab.spumPackages = packages;
        }
        // 패키지 체크
        Debug.Log($"Prefab Package { packages.Count } / Total Package { spumPackages.Count }");
        animationManager.PlayFirstAnimation();
    }
    
#endregion
    
#region ElementData
    private bool AreElementsEqual(PreviewMatchingElement element1, PreviewMatchingElement element2)
    {
        return element1.UnitType == element2.UnitType &&
            element1.PartType == element2.PartType &&
            element1.Dir == element2.Dir &&
            element1.Structure == element2.Structure
            ;
        // 필요한 만큼 조건을 추가할 수 있습니다.
    }
    public void SaveElementData(List<PreviewMatchingElement> ElementList)
    {
        var PreviewUnit = PreviewPrefab;
        foreach (var newElement in ElementList)
        {
            var existingElement = PreviewUnit.ImageElement.FirstOrDefault(e => AreElementsEqual(e, newElement));

            if (existingElement != null)
            {
                int index = PreviewUnit.ImageElement.IndexOf(existingElement);
                PreviewUnit.ImageElement[index] = newElement;
            }
            else
            {
                PreviewUnit.ImageElement.Add(newElement);
            }
        }
    }
    public void RemoveElementData(List<PreviewMatchingElement> ElementList)
    {
        var PreviewUnit = PreviewPrefab;
        foreach (var newElement in ElementList)
        {
            var existingElement = PreviewUnit.ImageElement.FirstOrDefault(e => AreElementsEqual(e, newElement));

            if (existingElement != null)
            {
                int index = PreviewUnit.ImageElement.IndexOf(existingElement);
                PreviewUnit.ImageElement.RemoveAt(index);
            }
        }
    }
    public void SetElementColorData(List<PreviewMatchingElement> ElementList)
    {
         var PreviewUnit = PreviewPrefab;
        foreach (var newElement in ElementList)
        {
            var existingElement = PreviewUnit.ImageElement.FirstOrDefault(e => AreElementsEqual(e, newElement));

            if (existingElement != null)
            {
                int index = PreviewUnit.ImageElement.IndexOf(existingElement);
                var ElementData = PreviewUnit.ImageElement[index];
                ElementData.Color = newElement.Color;
            }
        }
    }
    public void SetElementMaskData(List<PreviewMatchingElement> ElementList)
    {
         var PreviewUnit = PreviewPrefab;
        foreach (var newElement in ElementList)
        {
            var existingElement = PreviewUnit.ImageElement.FirstOrDefault(e => AreElementsEqual(e, newElement));

            if (existingElement != null)
            {
                int index = PreviewUnit.ImageElement.IndexOf(existingElement);
                var ElementData = PreviewUnit.ImageElement[index];
                ElementData.MaskIndex = newElement.MaskIndex;
            }
        }
    }

#endregion

#region Prefab


    //프리팹 저장 부분
    public void SavePrefabs()
    {
        animationManager.CloseAnimationPanels();
        var SpumPreviewUnit = PreviewPrefab;
        string prefabName = UIManager._unitCode.text;

        SpumPreviewUnit._code = prefabName;
        //SpumPreviewUnit.EditChk = false;
        
        GameObject prefabs = Instantiate(SpumPreviewUnit.gameObject);
        SPUM_Prefabs SpumUnitData = prefabs.GetComponent<SPUM_Prefabs>();
        SpumUnitData.ImageElement = SpumPreviewUnit.ImageElement;
        SpumUnitData.spumPackages = SpumPreviewUnit.spumPackages;
        // 비활성화된 오브젝트 삭제하기
        var inactiveObjects = prefabs.transform.Cast<Transform>()
            .Where(child => !child.gameObject.activeInHierarchy)
            .Select(child => child.gameObject)
            .ToList();

        inactiveObjects.ForEach(DestroyImmediate);
        
        prefabs.transform.localScale = Vector3.one;
        SpumUnitData._anim = prefabs.GetComponentInChildren<Animator>();
        SpumUnitData._anim.runtimeAnimatorController = SPUM_AnimatorDic[SpumPreviewUnit.UnitType];
        SpumUnitData._version = _version;
        SpumUnitData.PopulateAnimationLists();
        if (!Directory.Exists(unitPath))
        {
            Directory.CreateDirectory(unitPath);
            AssetDatabase.Refresh();
            Debug.Log("Folder created at: " + unitPath);
        }  
        GameObject SavePrefab = PrefabUtility.SaveAsPrefabAsset(prefabs,unitPath+prefabName+".prefab");
        DestroyImmediate(prefabs);
        
        UIManager.ToastOn("Saved Unit Object " + prefabName);
        //초기화
        var Prefab = SavePrefab.GetComponent<SPUM_Prefabs>();
        //Prefab.PopulateAnimationLists();
        paginationManager.AddNewPrefab(Prefab);
        SpumPreviewUnit._code = "";
        UIManager.ResetUniqueID();
        //SpumPreviewUnit.EditChk = true;

        UIManager.ShowNowUnitNumber();
        NewMake();
    }

    //프리펩 수정 부분
    public void EditPrefabs()
    {
        var SpumPreviewUnit = PreviewPrefab;

        string prefabCode = SpumPreviewUnit._code;

        SPUM_Prefabs PreviewUnit = SpumPreviewUnit.GetComponent<SPUM_Prefabs>();

        //SpumPreviewUnit._code = prefabName;
        SpumPreviewUnit._version = _version;
        //SpumPreviewUnit.EditChk = false;

        GameObject prefabs = Instantiate(SpumPreviewUnit.gameObject);
        SPUM_Prefabs SpumUnitData = prefabs.GetComponent<SPUM_Prefabs>();
        SpumUnitData.ImageElement = SpumPreviewUnit.ImageElement;
        SpumUnitData.spumPackages = SpumPreviewUnit.spumPackages;

        // 비활성화된 오브젝트 삭제하기
        var inactiveObjects = prefabs.transform.Cast<Transform>()
            .Where(child => !child.gameObject.activeInHierarchy)
            .Select(child => child.gameObject)
            .ToList();

        inactiveObjects.ForEach(DestroyImmediate);

        prefabs.transform.localScale = Vector3.one;
        SpumUnitData._anim = prefabs.GetComponentInChildren<Animator>();
        SpumUnitData._anim.runtimeAnimatorController = SPUM_AnimatorDic[SpumPreviewUnit.UnitType];

        var sourcePath = AssetDatabase.GetAssetPath(EditPrefab);
        Debug.Log(sourcePath);
        if(string.IsNullOrWhiteSpace(sourcePath)) 
        {
            sourcePath = Path.Combine(unitPath,SpumUnitData._code );
        }
        var FileName = sourcePath.Split("/");
        var path = isSaveSamePath ? sourcePath.Replace(FileName[FileName.Length-1], "") : unitPath;
        SpumUnitData.PopulateAnimationLists();
        GameObject SavePrefab = PrefabUtility.SaveAsPrefabAsset(prefabs,path+SpumUnitData._code+".prefab");
        // //GameObject SavePrefab = PrefabUtility.SaveAsPrefabAsset( prefabs, unitPath + prefabCode + ".prefab" );
        // var Prefab = SavePrefab.GetComponent<SPUM_Prefabs>();
        // Prefab.PopulateAnimationLists();
        DestroyImmediate(prefabs);

        PreviewUnit._code = "";
        UIManager.ResetUniqueID();
        //PreviewUnit.EditChk = true;

        UIManager.ToastOn("Edited Unit Object Unit" + prefabCode);

        NewMake();
    }

    public void NewMake()
    {
        ItemResetAll();
        EditPrefab = null;
        //UIManager._unitCode.text = UIManager.GetFileName();
        UIManager.LoadButtonSet(false);
        animationManager.CloseAnimationPanels();
        UIManager.CloseColorPick();
        animationManager.InitPreviewUnitPackage();
        ResetBody();
    }
    //프리팹 프리펩 리스트 불러오기
    public void OpenLoadData()
    {
        // 애니메이션 패널 닫기
        animationManager.CloseAnimationPanels();
        animationManager.InitializeDropdown();
        // 로드 오브젝트 캔버스 활성화
        paginationManager.LoadPrefabs();

        UIManager.SetActiveLoadPanel(true);
    }
    public List<PreviewMatchingElement> DebugList = new List<PreviewMatchingElement>();
    public List<string> MissingPackageNames = new List<string>();
    public SPUM_Prefabs previewUnit;
    public void SetUnitConverter(string Type)
    {
        int UnitBodyCount = DebugList.Count(element => element.PartType == "Body");
        if(UnitBodyCount < 6)
        {
            DebugList.AddRange(DefaultData("Unit", "Body", "Human_1", Color.white));
        }
        UIManager.ConvertView.WarningText.SetActive(UnitBodyCount < 6);
        //Debug.Log("UnitBodyCount " + UnitBodyCount);
        int UnitEyeCount = DebugList.Count(element => element.PartType == "Eye");
        UIManager.ConvertView.WarningEyeText.SetActive(UnitEyeCount < 2);
        if(UnitEyeCount < 2){
            DebugList.AddRange(DefaultData("Unit", "Eye", "Eye0", new Color32(71, 26,26, 255)));
        }
        var DistinctPackageList = MissingPackageNames.Distinct().ToList();
        MissingPackageNames = DistinctPackageList;

        UIManager.ConvertView.MissingPackageNames.transform.parent.gameObject.SetActive(MissingPackageNames.Count > 0);
        if(MissingPackageNames.Count > 0){
            string Text = "";
            foreach (var item in MissingPackageNames)
            {
                Text += "\n-" + item ;
            }
           
            string format = $"Missing\nPackages\n--------------{ Text }";
            UIManager.ConvertView.MissingPackageNames.text = format;
        }
        var containUnitTypes = DebugList
        .Select(e => e.UnitType)
        .Distinct()
        .ToList();
        bool shouldActivate = containUnitTypes.Any(unitType => unitType.Contains("Horse"));
        if(shouldActivate) Type = "Horse";
        previewUnit.UnitType = Type;
        foreach (Transform child in previewUnit.transform)
        {
            child.gameObject.SetActive(child.name.Contains(Type));
        }
        var anim = previewUnit.GetComponentInChildren<Animator>();
        previewUnit._anim = anim;

        var matchingTables = previewUnit.GetComponentsInChildren<SPUM_MatchingList>(true);
        var allMatchingElements = matchingTables.SelectMany(mt => mt.matchingTables).ToList();
        foreach (var matchingElement in allMatchingElements)
        {
            if (matchingElement.renderer != null)
            {
                matchingElement.renderer.sprite = null;
                matchingElement.renderer.maskInteraction = SpriteMaskInteraction.None;
                matchingElement.renderer.color = Color.white;
                matchingElement.ItemPath = "";
                matchingElement.Color = Color.white;
            }
        }
        foreach (var matchingElement in allMatchingElements)
        {
            var matchingTypeElement = DebugList.FirstOrDefault(ie => 
            (ie.UnitType == matchingElement.UnitType)
            && (ie.PartType == matchingElement.PartType)
            //&& ie.Index == matchingElement.Index
            && (ie.Dir == matchingElement.Dir)
            && (ie.Structure == matchingElement.Structure) 
            && ie.PartSubType == matchingElement.PartSubType
            );
            //Debug.Log(matchingTypeElement != null);
            if (matchingTypeElement != null)
            {
                var LoadSprite = LoadSpriteFromMultiple(matchingTypeElement.ItemPath , matchingTypeElement.Structure);
                matchingElement.renderer.sprite = LoadSprite;
                matchingElement.renderer.maskInteraction = (SpriteMaskInteraction)matchingTypeElement.MaskIndex;
                matchingElement.renderer.color = matchingTypeElement.Color; 
                matchingElement.ItemPath = matchingTypeElement.ItemPath;
                matchingElement.MaskIndex = matchingTypeElement.MaskIndex;
                matchingElement.Color = matchingTypeElement.Color;
                //Debug.Log( matchingTypeElement.PartType + "/" + matchingTypeElement.Color);
            }
        }


        previewUnit.ImageElement = DebugList;

        
        //애니메이션 경로 체크
        if(previewUnit.spumPackages.Count > 0){
            bool clipPathExists = true;
            var ClipList = previewUnit.spumPackages.SelectMany(package => package.SpumAnimationData).ToList();
            
            foreach (var clip in ClipList)
            {
                clipPathExists = ValidateAnimationClips(clip);
                if(!clipPathExists){
                    // 패키지 네임 // 애니메이션 타입 // 애니메이션 이름
                    var dataArray = clip.ClipPath.Split("/");

                    var PackageDataName = dataArray[0];
                    if(PackageDataName.Equals("Addons")){
                        PackageDataName = dataArray[1];
                    }
                    var PackageNameExist = SpritePackageNameList.Contains(PackageDataName);
                    if(!PackageNameExist)
                    {
                        //Debug.Log("MissingPackage");
                        MissingPackageNames.Add(PackageDataName);
                    }
                    var PackageName = PackageNameExist ? PackageDataName : "";
                    var ClipName = dataArray[dataArray.Length-1];
                    var ExtractList = ExtractTextureData(PackageName, clip.UnitType, clip.StateType, ClipName);
                    var data = ExtractList.FirstOrDefault();
                    if(data != null){
                    Debug.Log($" Package {PackageNameExist} {PackageName} {ClipName} {data.Name} {data.Path}");
                        
                        clip.ClipPath = data.Path;
                    }
                }
            }

        }else{
            //애니메이션 데이터 초기화
            previewUnit.spumPackages = GetSpumLegacyData();
        }
    }
    public bool ValidateAnimationClips(SpumAnimationClip clipData)
    {
        bool clipPathExists = true;
        // #if UNITY_EDITOR
        // var asset = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipData.ClipPath.Replace(".anim", ""));
        // if (asset == null)
        // {
        //     Debug.LogWarning($"Failed to load animation clip '{clipData.ClipPath}'.");
        //     clipPathExists = false;
        // }
        // #else
        
        AnimationClip LoadClip = Resources.Load<AnimationClip>(clipData.ClipPath.Replace(".anim", ""));
        
        if (LoadClip == null)
        {
            Debug.LogWarning($"Failed to load animation clip '{clipData.ClipPath}'.");
            clipPathExists = false;
        }
        return clipPathExists;
    }
    
    public SPUM_Prefabs SaveConvertPrefabs(UnityEngine.Object asset)
    {
        var SpumPreviewUnit = PreviewPrefab;
        string prefabName = UIManager._unitCode.text;

        SpumPreviewUnit._code = prefabName;
        //SpumPreviewUnit.EditChk = false;
        
        GameObject prefabs = Instantiate(previewUnit.gameObject);
        SPUM_Prefabs SpumUnitData = prefabs.GetComponent<SPUM_Prefabs>();
        SpumUnitData.ImageElement = DebugList;
        SpumUnitData.spumPackages = SpumPreviewUnit.spumPackages;
        // 비활성화된 오브젝트 삭제하기
        
        prefabs.transform.localScale = Vector3.one;
        prefabs.transform.position = Vector3.zero;
        SpumUnitData._version = _version;
        var UniqueID = System.DateTime.Now.ToString("yyyyMMddHHmmssfff");
        SpumUnitData._code = "SPUM" + "_" + UniqueID;
        SpumUnitData._anim.Rebind();
        var sourcePath = AssetDatabase.GetAssetPath(asset);
        var FileName = sourcePath.Split("/");
        var path = isSaveSamePath ? sourcePath.Replace(FileName[FileName.Length-1], "") : unitPath;
        Debug.Log(sourcePath.Replace(asset.name+".prefab", "").Replace(asset.name+".Prefab", "")  );
        GameObject SavePrefab = PrefabUtility.SaveAsPrefabAsset(prefabs,path+SpumUnitData._code+".prefab");
        DestroyImmediate(prefabs);
        AssetDatabase.Refresh();
        UIManager.ToastOn("Saved Unit Object " + prefabName);
        //초기화
        SpumPreviewUnit._code = "";
        DebugList.Clear();
        var Prefab = SavePrefab.GetComponent<SPUM_Prefabs>();
        Prefab.PopulateAnimationLists();
        return Prefab;
        
    }
    public void MoveOldPrefabBackup(UnityEngine.Object asset)
    {
        var sourcePath = AssetDatabase.GetAssetPath(asset);
        if (!Directory.Exists(unitBackUpPath))
        {
            Directory.CreateDirectory(unitBackUpPath);
            AssetDatabase.Refresh();
            Debug.Log("Folder created at: " + unitBackUpPath);
        }  
        var destinationPath = unitBackUpPath+asset.name+"_Backup.Prefab";
        AssetDatabase.MoveAsset(sourcePath, destinationPath);
        AssetDatabase.Refresh();
    }
    public List<PreviewMatchingElement> SetLegacyHorseData(){
        string PackageName = "Legacy";
        string UnitType = "Horse";
        string PartType = "Body";
        string TextureName = "Horse1";
        //패키지 구룹화
        var groupedData = spumPackages
            .SelectMany(p => p.SpumTextureData.Select(t => new { Package = p, Texture = t }))
            .Where(x => x.Texture.PartType.Equals(PartType) && x.Texture.UnitType.Equals(UnitType) && x.Package.Name.Equals(PackageName) && x.Texture.Name.Equals(TextureName))
            .GroupBy(x => new { x.Texture.PartType, x.Texture.Name, x.Texture.UnitType }) // 그룹 키
            .Select(g => new 
            { 
                Key = g.Key, 
                Items = g.ToList() 
            })
            .ToList();

        var randomGroup = groupedData[0];

        var ListElement = new List<PreviewMatchingElement>();
        foreach (var item in randomGroup.Items)
        {
            //Debug.Log($"Path: {PartType}, SubType: { item.Texture.SubType}");
            var part = new PreviewMatchingElement();
            part.UnitType = UnitType;
            part.PartType = PartType;
            part.PartSubType = item.Texture.PartSubType;
            part.Dir = "";
            part.ItemPath = item.Texture.Path;
            part.Structure = item.Texture.SubType.Equals(item.Texture.Name) ? PartType : item.Texture.SubType;
            part.MaskIndex = 0;
            part.Color = Color.white;

            ListElement.Add(part);
        }
        return ListElement;
    }
    public (int, List<PreviewMatchingElement>) ValidateSpumFile(SPUM_Prefabs PrefabObject)
    {
        var SpumPrefab = PrefabObject;
        var version = SpumPrefab._version;
        var UnitType =  SpumPrefab.UnitType;
        var MatchingList = SpumPrefab.GetComponentsInChildren<SPUM_MatchingList>();
        bool isMatchingListExist = MatchingList != null || MatchingList.Length > 0; // 2.0 시스템
        bool isVersionSame = SpumPrefab._version == version;
        var NewDataListElement = new List<PreviewMatchingElement>();
        var OldData = SpumPrefab.GetComponentInChildren<SPUM_SpriteList>(); // 1.0 시스템
        if(OldData == null) {
            //DebugList.AddRange(PrefabObject.ImageElement);
            return (2, PrefabObject.ImageElement);
        }
        var horseString = OldData._spHorseString;

        var path = AssetDatabase.GetAssetPath(PrefabObject);
        Debug.Log(path);
        bool HorseExist = !string.IsNullOrWhiteSpace(horseString);
        // var horseList = OldData._spHorseSPList._spList;
        // var HorseBodySet = new List<PreviewMatchingElement>();
        // foreach (var renderer in horseList)
        // {
        //     HorseBodySet.AddRange(StringToSpumElementList("Horse", (horseString, renderer)));
        // }
        // NewDataListElement.AddRange(HorseBodySet);
        if(HorseExist){
            var horseReset = SetLegacyHorseData();
            NewDataListElement.AddRange(horseReset);
        }

        string Unitype = "Unit";

        // 메인 바디 
        


        var hairString = OldData._hairListString;
        var hairList = OldData._hairList;
        var TuppleHair = CreateTupleList(hairString, hairList);
        //LoopStringColor(TuppleHair);
        var MaskSet = new List<PreviewMatchingElement>();
        foreach (var tuple in TuppleHair)
        {
            // 투구 및 헤어
            MaskSet.AddRange(StringToSpumElementList(Unitype, tuple));
        }
        //Debug.Log("count " + MaskSet.Count);
        List<string> requiredPartTypes = new List<string> { "Hair", "Helmet"};
        bool result = requiredPartTypes.All(partType => MaskSet.Any(element => element.PartType == partType));
        if(result) 
        {
            foreach (var item in MaskSet)
            {
                if(item.PartType.Equals("Hair")) item.MaskIndex = 1;
            }
        }

        NewDataListElement.AddRange(MaskSet);
        var clothString = OldData._clothListString;
        var clothList = OldData._clothList;
        var TuppleCloth = CreateTupleList(clothString, clothList);
        foreach (var tuple in TuppleCloth)
        {
            NewDataListElement.AddRange(StringToSpumElementList(Unitype, tuple));
        }

        var armorString = OldData._armorListString;
        var armorList = OldData._armorList;
        var TuppleArmor = CreateTupleList(armorString, armorList);
        foreach (var tuple in TuppleArmor)
        {
            NewDataListElement.AddRange(StringToSpumElementList(Unitype, tuple));
        }

        var pantString = OldData._pantListString;
        var pantList = OldData._pantList;
        var TupplePant = CreateTupleList(pantString, pantList);
        foreach (var tuple in TupplePant)
        {
            NewDataListElement.AddRange(StringToSpumElementList(Unitype, tuple));
        }

        var weaponString = OldData._weaponListString;
        var weaponList = OldData._weaponList;
        var TuppleWeapon = CreateTupleList(weaponString, weaponList);
        foreach (var tuple in TuppleWeapon)
        {
            // 예외 케이스 설정 / 왼쪽 오른쪽
            var WeaponsData = StringToSpumElementList(Unitype, tuple);
            NewDataListElement.AddRange(WeaponsData);
        }

        var backString = OldData._backListString;
        var backList = OldData._backList;
        var TuppleBack = CreateTupleList(backString, backList);
        foreach (var tuple in TuppleBack)
        {
            NewDataListElement.AddRange(StringToSpumElementList(Unitype, tuple));
        }



        var bodyString = OldData._bodyString;
        var bodyList = OldData._bodyList;
        var BodySet = new List<PreviewMatchingElement>();
        foreach (var renderer in bodyList)
        {
            BodySet.AddRange(StringToSpumElementList(Unitype, (bodyString, renderer)));
        }
        // if(!BodySet.Count.Equals(6))
        // {
        //     BodySet.AddRange(DefaultData("Unit", "Body", "Human_1", Color.white));
        // }
        // UIManager.ConvertView.WarningText.SetActive(BodySet.Count < 6);
        //Debug.Log(BodySet.Count + " ======= Body Count");
        NewDataListElement.AddRange(BodySet);

        //DefaultData("Unit", "Eye", "Eye0", new Color32(71, 26,26, 255));
        var eyeString = "";
        var eyeList = OldData._eyeList; 
        var EyeColorSet = new List<PreviewMatchingElement>();
        foreach (var renderer in eyeList)
        {
            EyeColorSet.AddRange(StringToSpumElementList(Unitype, (eyeString, renderer)));
        }
    
        var EyeDistict = EyeColorSet.Distinct().GroupBy(x => new { x.Structure }).Select(g => g.First()).ToList();
        //Debug.Log(EyeDistict.Count + " ======= EyeDistict Count");
        // UIManager.ConvertView.WarningEyeText.SetActive(EyeDistict.Count.Equals(0));
        // if(EyeDistict.Count.Equals(0)){
        //     EyeDistict.AddRange(DefaultData("Unit", "Eye", "Eye0", new Color32(71, 26,26, 255)));
        // }
        foreach (var item in EyeDistict)
        {
            foreach (var sprite in eyeList)
            {
                if(sprite.name.Equals(item.Structure)) 
                { 
                    item.Color = sprite.color; 
                }
            }
        }
        // foreach (var item in EyeDistict)
        // {
        //     if(item.PartType.Equals("Eye")) Debug.Log(item.Color);
        // }
        NewDataListElement.AddRange(EyeDistict);
        //Debug.Log("Unit? " + string.IsNullOrWhiteSpace(horseString));
        
        var distinct = NewDataListElement.Distinct()
        .GroupBy(x => new { x.UnitType, x.PartType, x.Structure, x.Dir })
            .Select(g => g.First())
            .ToList();
        //Debug.Log( " distinct.Count " + distinct.Count);
        //DebugList.AddRange(distinct);
        return (1, distinct);
        // 버전이 다르거나, 구조가 다르거나, 컴포넌트가 없거나 
        // 체크 유닛 타입
        // 체크 패키지 버전
        // 재구축
        //GameObject prefabs = Instantiate(SpumPreviewUnit.gameObject);
        // GameObject tObj = PrefabUtility.SaveAsPrefabAsset(prefabs,unitPath+prefabName+".prefab");
        // DestroyImmediate(prefabs);
    }
    public List<PreviewMatchingElement> DefaultData(string UnitType, string PartType, string TextureName, Color color)
    {
        string PackageName ="Legacy";
        //패키지 구룹화
        var groupedData = spumPackages
            .SelectMany(p => p.SpumTextureData.Select(t => new { Package = p, Texture = t }))
            .Where(x => x.Texture.PartType.Equals(PartType) && x.Texture.UnitType.Equals(UnitType) && x.Package.Name.Equals(PackageName) && x.Texture.Name.Equals(TextureName))
            .GroupBy(x => new { x.Texture.PartType, x.Texture.Name, x.Texture.UnitType }) // 그룹 키
            .Select(g => new 
            { 
                Key = g.Key, 
                Items = g.ToList() 
            })
            .ToList();

        var randomGroup = groupedData[0];

        var ListElement = new List<PreviewMatchingElement>();
        foreach (var item in randomGroup.Items)
        {
            //Debug.Log($"Path: {PartType}, SubType: { item.Texture.SubType}");
            var part = new PreviewMatchingElement();
            part.UnitType = UnitType;
            part.PartType = PartType;
            part.PartSubType = item.Texture.PartSubType;
            part.Dir = "";
            part.ItemPath = item.Texture.Path;
            part.Structure = item.Texture.SubType.Equals(item.Texture.Name) ? PartType : item.Texture.SubType; // item.Texture.SubType.Equals(item.Texture.Name) ? PartType : item.Texture.SubType;
            part.MaskIndex = 0;
            part.Color = color;

            ListElement.Add(part);
        }
        return ListElement;
    }
    public List<PreviewMatchingElement> StringToSpumElementList(string UnitType, (string, SpriteRenderer) Tuple)
    {
        var PartPath = Tuple.Item1;
        string unitType = UnitType;
        //bool isPackage = PartPath.Contains("Packages");
        string PackageName = "Legacy";
        string pattern = @"Packages\/([^\/]+)\/";
        // 패키지는 없지만 이미지 이름은 있는 경우
        bool isPackage = false;
        
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(PartPath, pattern);
        if (match.Success)
        {
            PackageName = match.Groups[1].Value;
            isPackage = true;
        }
        if(PackageName.Equals("Heroes")) PackageName = "RetroHeroes";
        bool missingPackage = isPackage && !SpritePackageNameList.Contains(PackageName);
        if(missingPackage)
        {
            //예외 처리
            
            //Debug.Log("MissingPackage");
            MissingPackageNames.Add(PackageName);
        }

        // 경로가 없지만 이미지 리소스는 있는경우 , 패키지 이름은 매칭되지만 패키지 리스트에 없는 경우
        if( ((PartPath == "") && (Tuple.Item2.sprite != null)) || missingPackage){
            var path = AssetDatabase.GetAssetPath(Tuple.Item2.sprite);
            //Assets/SPUM/Resources/Elf/0_Unit/0_Sprite/0_Body/New_Elf_1.png
            PartPath = path;
            //Debug.Log(path);
            string pattern2 = @"Addons\/(.*?)\/0_Unit";

            // 등록된 이미지 리소스 경로로 매칭 시작
            System.Text.RegularExpressions.Match match2 = System.Text.RegularExpressions.Regex.Match(PartPath, pattern2);
            if (match2.Success)
            {
                PackageName = match2.Groups[1].Value;
            }
            

        }
        if(string.IsNullOrWhiteSpace(PartPath)) return new List<PreviewMatchingElement>();
        //var SpriteRendererData = Tuple.Item2;
        


        var PathArray =  PartPath.Split("/");
        string PartType = System.Text.RegularExpressions.Regex.Replace(PathArray[PathArray.Length-2],@"[^a-zA-Z가-힣\s]", "");
        // if(Tuple.Item1 != "") {
        //     Debug.Log("=====================" +Tuple.Item1);
        //     var PathArray2 = Tuple.Item1.Split('/');
        //     PartType = System.Text.RegularExpressions.Regex.Replace(PathArray2[PathArray2.Length-2],@"[^a-zA-Z가-힣\s]", "");
        //     Debug.Log("=====================" +PartType);
        // }
        string NoNamePackagePartType = System.Text.RegularExpressions.Regex.Replace(PathArray[PathArray.Length-3],@"[^a-zA-Z가-힣\s]", "");
        PartType = PartPath.Contains("BodySource") ? "Body" : NoNamePackagePartType.Equals("Weapons") ? "Weapons" : PartType; // 구 바디 예외

        // string NoNamePackagePartType = System.Text.RegularExpressions.Regex.Replace(PathArray[PathArray.Length-2],@"[^a-zA-Z가-힣\s]", "");
        // PartType = PartPath.Contains("BodySource") ? "Body" :  PartType; // 구 바디 예외
        string PartName = System.Text.RegularExpressions.Regex.Replace(PathArray[PathArray.Length-1], @"\..*", "");
        //Debug.Log(PackageName+"/"+unitType +"/"+ PartType+"/"+PartName + "/" + NoNamePackagePartType);
        if(NoNamePackagePartType.Equals("BasicResources")) 
        {
            PartType = PartType.Replace("Backup", "");
        }
        var dir = "";
        bool isHide = false;
        if(PartType.Equals("Helmet"))
        {
            if(Tuple.Item2.name == "12_Helmet2") { dir = "Front"; isHide = Tuple.Item1 == ""; }
            if(Tuple.Item2.name == "11_Helmet1") { dir = "Front"; isHide = Tuple.Item1 == ""; }
        }


        if(PartType.Equals("Weapons"))
        {
            if(Tuple.Item2.name == "R_Weapon") { dir = "Right"; isHide = Tuple.Item1 == ""; }
            if(Tuple.Item2.name == "R_Shield") { dir = "Right"; isHide = Tuple.Item1 == ""; }
            if(Tuple.Item2.name == "L_Weapon") { dir = "Left";  isHide = Tuple.Item1 == ""; }
            if(Tuple.Item2.name == "L_Shield") { dir = "Left";  isHide = Tuple.Item1 == ""; }
            // r weapon
            // r shield
            // l weapon
            // l shield
            //Debug.Log(PartName + "/" +dir);
        }
        //Debug.Log(PackageName + " : " + unitType+ " : " + PartType+ " : " + PartName);
        //패키지 구룹화
        var ExtractList = ExtractTextureData(PackageName, unitType, PartType, PartName);
        
        var ListElement = new List<PreviewMatchingElement>();
        foreach (var item in ExtractList)
        {
            //PartColor = Tuple.Item2.color.Equals(Color.white) ? PartColor : Tuple.Item2.color;
            //Debug.Log($"{ item.Name } { item.UnitType } { item.PartType } { item.PartSubType }");
            //Debug.Log($"Path: {PartType}, SubType: { item.SubType}");
            var part = new PreviewMatchingElement();
            part.UnitType = UnitType;
            part.PartType = PartType;
            part.PartSubType = item.PartSubType;
            part.Dir = dir;//ButtonData.Direction;
            part.ItemPath =  isHide ? "" : item.Path;
            part.Structure = item.SubType.Equals(item.Name) ? PartType : item.SubType;
            part.MaskIndex = 0;//(int)ButtonData.SpriteMask;
            part.Color = Tuple.Item2.color;//ButtonData.PartSpriteColor;
            //Debug.Log(PartType + "/" +Tuple.Item2.color.ToString());

            ListElement.Add(part);
        }
        return ListElement;
    }
    public List<PreviewMatchingElement> ReSyncSpumElementDataList(List<PreviewMatchingElement> List)
    {
        // 패키지 이름 존재 여부 2버전
        // 패키지 이름이 없으면 불가능 오브젝트로 이동동
        var ModifiyList = new List<PreviewMatchingElement>();
        foreach (var oldData in List)
        {
            var dataArray = oldData.ItemPath.Split("/");

            var PackageDataName = dataArray[0];
            if(PackageDataName.Equals("Addons")){
                PackageDataName = dataArray[1];
            }
            var PackageNameExist = SpritePackageNameList.Contains(PackageDataName);
            if(!PackageNameExist)
            {
                //Debug.Log("MissingPackage");
                MissingPackageNames.Add(PackageDataName);
            }
            var PackageName = PackageNameExist ? PackageDataName : "";
            var PartName = dataArray[dataArray.Length-1];
            var ExtractList = ExtractTextureData(PackageName, oldData.UnitType, oldData.PartType, PartName);
            var data = ExtractList.FirstOrDefault();
            //Debug.Log($" Package" + oldData.PartType + "/" + PartName);
            if(data != null){
            //Debug.Log($" Package {PackageNameExist} {PackageName} {PartName} {data.Name} {data.Path}");
                if(oldData.PartType.Equals("Weapons"))
                {
                    var PathArray = data.Path.Split("/");
                    string PartType = System.Text.RegularExpressions.Regex.Replace(PathArray[PathArray.Length-2],@"[^a-zA-Z가-힣\s]", "");
                    oldData.PartSubType = PartType;
                }
                
                oldData.ItemPath = data.Path;
                oldData.Color = oldData.Color.Equals(Color.clear) ? Color.white : oldData.Color;
                ModifiyList.Add(oldData);
            }
        }
        return ModifiyList;
    }

    List<(string, SpriteRenderer)> CreateTupleList(List<string> stringList, List<SpriteRenderer> spriteRendererList)
    {
        // 두 리스트의 길이가 다를 경우 짧은 쪽에 맞춥니다.
        int minLength = Mathf.Min(stringList.Count, spriteRendererList.Count);

        // LINQ를 사용하여 튜플 리스트 생성
        return stringList.Take(minLength)
                         .Zip(spriteRendererList.Take(minLength), (s, sr) => (s, sr))
                         .ToList();
    }
    public List<SpumTextureData> ExtractTextureData(string packageName, string unitType, string partType, string textureName)
    {
        var query = spumPackages.AsEnumerable();

        if (!string.IsNullOrEmpty(packageName))
        {
            query = query.Where(package => package.Name == packageName);
        }

        return query
            .SelectMany(package => package.SpumTextureData)
            .Where(texture => 
                texture.UnitType == unitType &&
                texture.PartType == partType &&
                texture.Name == textureName)
            .ToList();
    }
    public List<SpumAnimationClip> ExtractAnimationData(string packageName, string unitType, string partType, string clipeName)
    {
        var query = spumPackages.AsEnumerable();

        if (!string.IsNullOrEmpty(packageName))
        {
            query = query.Where(package => package.Name == packageName);
        }

        return query
            .SelectMany(package => package.SpumAnimationData)
            .Where(clip => 
                clip.UnitType == unitType &&
                clip.StateType == partType &&
                clip.Name == clipeName)
            .ToList();
    }

    //Unit Delete
    public void DeleteUnit(UnityEngine.Object prefab)
    {
        string pathToDelete = AssetDatabase.GetAssetPath(prefab);
        Debug.Log(pathToDelete); 
        AssetDatabase.DeleteAsset(pathToDelete);

        UIManager.ShowNowUnitNumber();
        UIManager.SetActiveLoadPanel(false);
        OpenLoadData();
    }
#endregion

    public Sprite LoadSpriteFromMultiple(string path, string spriteName)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found at path: {path}");
            return null;
        }

        Sprite foundSprite = System.Array.Find(sprites, sprite => sprite.name == spriteName);

        // 일치하는 spriteName이 없으면 첫 번째 항목 반환
        return foundSprite != null ? foundSprite : sprites[0];
    }
    //Resolve
    public void CheckVesionFile()
    {
        if(File.Exists("Assets/SPUM/Script/SPUM_TexutreList.cs"))
        {
            Debug.Log("Filex Exits, will delete it");
            FileUtil.DeleteFileOrDirectory("Assets/SPUM/Script/SPUM_TexutreList.cs");
        }
    }
    #endif
    //Package 
}
