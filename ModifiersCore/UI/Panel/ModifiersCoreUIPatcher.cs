using System.Linq;
using HMUI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ModifiersCore;

internal class ModifiersCoreUIPatcher : MonoBehaviour {
    #region Setup

    public GameplayModifiersPanelController Panel { get; private set; } = null!;
    public RectTransform ModifiersSection { get; private set; } = null!;
    public RectTransform CustomModifiersContainer { get; private set; } = null!;
    public RectTransform CustomModifiersSection { get; private set; } = null!;
    public TextSegmentedControl TabSelector { get; private set; } = null!;

    private void Awake() {
        Panel = GetComponent<GameplayModifiersPanelController>();
        ModifiersSection = (RectTransform)transform.Find("Modifiers");
        PatchUI();
    }

    #endregion

    #region PatchUI

    private const float TabSelectorWidth = 60f;
    private const float TabSelectorHeight = 5f;
    private const float TabSelectorFontSize = 3.5f;
    //the vanilla layout has 4 spare units above the info and 2 below it, the tab selector takes them
    private const float InfoOffset = 1.5f;

    private void PatchUI() {
        var container = FindObjectsByType<Context>(FindObjectsSortMode.InstanceID).Last().Container;
        var setupViewController = GetComponentInParent<GameplaySetupViewController>(true);
        //moving the info down to fit the tab selector
        foreach (var name in new[] { "Info", "HintText" }) {
            var rect = (RectTransform)transform.Find(name);
            rect.anchoredPosition -= new Vector2(0f, InfoOffset);
        }
        CreateTabSelector(setupViewController, container);
        CreateCustomSection(setupViewController, container);
    }

    private void CreateTabSelector(GameplaySetupViewController setupViewController, DiContainer container) {
        TabSelector = Instantiate(setupViewController._selectionSegmentedControl, transform, false);
        TabSelector.name = "ModifiersCoreTabSelector";
        //the original might already have its cells created
        foreach (var child in TabSelector.transform.Cast<Transform>().ToArray()) {
            DestroyImmediate(child.gameObject);
        }
        container.Inject(TabSelector);
        //
        var rect = (RectTransform)TabSelector.transform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(TabSelectorWidth, TabSelectorHeight);
        rect.anchoredPosition = new Vector2(0f, -0.5f);
        //
        TabSelector._fontSize = TabSelectorFontSize;
        TabSelector.SetTexts(new[] { "Vanilla", "Custom" });
    }

    #endregion

    #region CustomSection

    private ManualScrollView _scrollView = null!;
    private RectTransform _scrollbarRect = null!;

    private void CreateCustomSection(GameplaySetupViewController setupViewController, DiContainer container) {
        //taking the place of the vanilla section
        CustomModifiersContainer = CreateRect("CustomModifiers", transform);
        CustomModifiersContainer.anchorMin = ModifiersSection.anchorMin;
        CustomModifiersContainer.anchorMax = ModifiersSection.anchorMax;
        CustomModifiersContainer.pivot = ModifiersSection.pivot;
        CustomModifiersContainer.anchoredPosition = ModifiersSection.anchoredPosition;
        CustomModifiersContainer.sizeDelta = ModifiersSection.sizeDelta;
        //creating scroll view
        var viewport = CreateRect("Viewport", CustomModifiersContainer);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.sizeDelta = Vector2.zero;
        viewport.gameObject.AddComponent<Touchable>();
        viewport.gameObject.AddComponent<RectMask2D>();
        _scrollView = viewport.gameObject.AddComponent<ManualScrollView>();
        //creating the section with the same grid as the vanilla one
        CustomModifiersSection = CreateRect("Content", viewport);
        CustomModifiersSection.anchorMin = new Vector2(0f, 1f);
        CustomModifiersSection.anchorMax = Vector2.one;
        CustomModifiersSection.pivot = new Vector2(0.5f, 1f);
        CustomModifiersSection.sizeDelta = Vector2.zero;
        var vanillaGrid = ModifiersSection.GetComponent<GridLayoutGroup>();
        var grid = CustomModifiersSection.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = vanillaGrid.cellSize;
        grid.spacing = vanillaGrid.spacing;
        grid.startCorner = vanillaGrid.startCorner;
        grid.startAxis = vanillaGrid.startAxis;
        //aligning cells with the vanilla columns, a centered grid would shift a row that is not full
        var width = ModifiersSection.rect.width;
        var columns = Mathf.FloorToInt((width + grid.spacing.x) / (grid.cellSize.x + grid.spacing.x));
        var padding = Mathf.RoundToInt((width - columns * grid.cellSize.x - (columns - 1) * grid.spacing.x) / 2f);
        grid.padding = new RectOffset(padding, padding, 0, 0);
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        var sizeFitter = CustomModifiersSection.gameObject.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        //setting up scroll view
        _scrollView._scrollViewDirection = ScrollView.ScrollViewDirection.Vertical;
        _scrollView._scrollType = ScrollView.ScrollType.FixedCellSize;
        _scrollView._fixedCellSize = grid.cellSize.y + grid.spacing.y;
        _scrollView._viewport = viewport;
        _scrollView._contentRectTransform = CustomModifiersSection;
        container.Inject(_scrollView);
        //
        AddScrollbar(setupViewController);
        _scrollView.ManualAwake();
        var contentUpdater = CustomModifiersSection.gameObject.AddComponent<ScrollContentSizeUpdater>();
        contentUpdater.scrollView = _scrollView;
        contentUpdater.scrollbar = _scrollbarRect.gameObject;
        contentUpdater.Refresh();
    }

    private void AddScrollbar(GameplaySetupViewController setupViewController) {
        var scrollbarPrefab = setupViewController
            .GetComponentsInChildren<RectTransform>(true)
            .FirstOrDefault(x => x.name == "ScrollBar") ?? FindObjectsByType<RectTransform>(FindObjectsSortMode.None).First(x => x.name == "ScrollBar");
        _scrollbarRect = Instantiate(scrollbarPrefab, CustomModifiersContainer, false);
        _scrollbarRect.gameObject.SetActive(true);
        //placing it next to the right column
        _scrollbarRect.anchorMin = new Vector2(1f, 0f);
        _scrollbarRect.anchorMax = Vector2.one;
        _scrollbarRect.pivot = new Vector2(0.5f, 0.5f);
        _scrollbarRect.sizeDelta = new Vector2(8f, 0f);
        _scrollbarRect.anchoredPosition = Vector2.zero;
        //setting up scroll view
        _scrollView._verticalScrollIndicator = _scrollbarRect.Find("VerticalScrollIndicator").GetComponent<VerticalScrollIndicator>();
        _scrollView._pageUpButton = _scrollbarRect.Find("UpButton").GetComponent<Button>();
        _scrollView._pageDownButton = _scrollbarRect.Find("DownButton").GetComponent<Button>();
    }

    private RectTransform CreateRect(string name, Transform parent) {
        var go = new GameObject(name) {
            layer = gameObject.layer
        };
        var rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    #endregion
}
