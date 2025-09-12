using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// UIView Sample Code
using Doozy.Runtime.UIManager.Containers;
//Back Button Sample Code
using Doozy.Runtime.UIManager.Input;
// Send Signal Sample Code
using Doozy.Runtime.Signals;

public class PageBase : MonoBehaviour
{
    protected UIView VW_Page;

    protected virtual void Awake(){
        VW_Page = GetComponent<UIView>();

        // Use Sample Code
        // VW_Page.OnShowCallback.Event.AddListener(() => {
            
        // });
        // VW_Page.OnHideCallback.Event.AddListener(() => {
            
        // });

        // Send Signal Sample Code
        //SignalsService.SendSignal(Category_Title, Message_Next);

        // Back Button Sample Code
        //BackButton.stream.SendSignal();
    }
}
