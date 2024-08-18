using HMUI;
using UnityEngine;

namespace ModifiersCore;

internal class ScrollContentSizeUpdater : MonoBehaviour {
    public ScrollView scrollView = null!;
    
    private void OnRectTransformDimensionsChange() {
        scrollView.UpdateContentSize();
    }
}