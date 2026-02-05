using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AutoScroll : MonoBehaviour
{
    ScrollRect scrollRect;
    
    void Start() 
    { 
        scrollRect = GetComponent<ScrollRect>(); 
    }
    
    public void ScrollToBottom() 
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(ScrollToBottomRoutine());
    }

    IEnumerator ScrollToBottomRoutine()
    {
        // Wait for the UI to resize the text box first
        yield return new WaitForEndOfFrame();
        
        Canvas.ForceUpdateCanvases();
        
        // Scroll to bottom
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
            
        // Double-tap: Sometimes one frame isn't enough for nested layouts
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
