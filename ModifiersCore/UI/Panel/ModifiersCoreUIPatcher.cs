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

    private void Awake() {
        Panel = GetComponent<GameplayModifiersPanelController>();
        ModifiersSection = (RectTransform)transform.Find("Modifiers");
        PatchUI();
    }

    #endregion

    #region PatchUI

    private ManualScrollView _scrollView = null!;
    private RectTransform _scrollbarRect = null!;

    private void PatchUI() {
        var context = FindObjectsOfType<Context>().Last();
        var container = context.Container;
        //creating scroll view
        var scrollContainer = new GameObject("ScrollContainer");
        scrollContainer.AddComponent<Touchable>();
        scrollContainer.AddComponent<RectMask2D>();
        var scrollRect = scrollContainer.GetComponent<RectTransform>();
        _scrollView = scrollContainer.AddComponent<ManualScrollView>();
        //
        scrollRect.SetParent(ModifiersSection.parent, false);
        scrollRect.sizeDelta = new Vector2(100f, 46f);
        scrollRect.pivot = new Vector2(0.5f, 1f);
        scrollRect.localPosition = new Vector2(0f, -16f);
        //setting up modifiers section
        ModifiersSection.SetParent(scrollRect, false);
        ModifiersSection.anchorMin = new Vector2(0f, 1f);
        ModifiersSection.anchorMax = Vector2.one;
        ModifiersSection.sizeDelta = Vector2.zero;
        ModifiersSection.pivot = new Vector2(0.5f, 1f);
        var contentUpdater = ModifiersSection.gameObject.AddComponent<ScrollContentSizeUpdater>();
        contentUpdater.scrollView = _scrollView;
        //
        var modifiersSizeFitter = ModifiersSection.gameObject.AddComponent<ContentSizeFitter>();
        modifiersSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        //setting up scroll view
        _scrollView._scrollViewDirection = ScrollView.ScrollViewDirection.Vertical;
        _scrollView._scrollType = ScrollView.ScrollType.FixedCellSize;
        _scrollView._fixedCellSize = 9.3f;
        _scrollView._viewport = scrollRect;
        _scrollView._contentRectTransform = ModifiersSection;
        container.Inject(_scrollView);
        //
        AddScrollbar();
        _scrollView.ManualAwake();
        SetupScrollbar();
    }

    private void AddScrollbar() {
        var scrollbarPrefab = FindObjectsOfType<RectTransform>().First(x => x.name == "ScrollBar");
        _scrollbarRect = Instantiate(scrollbarPrefab, Panel.transform, false);
        //setting up scroll view
        _scrollView._verticalScrollIndicator = _scrollbarRect.Find("VerticalScrollIndicator").GetComponent<VerticalScrollIndicator>();
        _scrollView._pageUpButton = _scrollbarRect.Find("UpButton").GetComponent<Button>();
        _scrollView._pageDownButton = _scrollbarRect.Find("DownButton").GetComponent<Button>();
    }

    private void SetupScrollbar() {
        //setting up scrollbar
        _scrollbarRect.pivot = Vector2.one * 0.5f;
        _scrollbarRect.sizeDelta = new Vector2(8f, -24f);
        _scrollbarRect.localPosition = new Vector2(50f, -39f);
        _scrollView.SetContentSize(46f);
    }

    #endregion
}