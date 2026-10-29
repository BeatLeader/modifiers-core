using HMUI;
using UnityEngine;

namespace ModifiersCore;

internal class ScrollContentSizeUpdater : MonoBehaviour {
    public ScrollView scrollView = null!;
    public GameObject? scrollbar;

    public void Refresh() {
        if (scrollView == null) return;
        scrollView.UpdateContentSize();
        //showing the scrollbar only when there is something to scroll
        if (scrollbar != null) {
            var content = (RectTransform)transform;
            var viewport = scrollView._viewport;
            scrollbar.SetActive(content.rect.height > viewport.rect.height + 0.01f);
        }
    }

    private void OnRectTransformDimensionsChange() {
        Refresh();
    }
}
