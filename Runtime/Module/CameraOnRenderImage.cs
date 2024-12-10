using UnityEngine;

public class CameraOnRenderImage : MonoBehaviour
{
    public RenderTexture renderTexture;

    // OnRenderImage 在相機渲染完圖像後會被調用
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // 將相機畫面拷貝到 RenderTexture 中
        Graphics.Blit(src, renderTexture);

        // 同時將畫面正常顯示在螢幕上
        Graphics.Blit(src, dest);
    }

    void OnDestroy()
    {
        // 當物件銷毀時，釋放 RenderTexture 資源
        renderTexture.Release();
    }
}
