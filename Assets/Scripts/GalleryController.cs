using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR;

public class GalleryController : MonoBehaviour
{
    #region Settings/Variables

    // --- Definitions ---
    [System.Serializable]
    public struct MediaElement
    {
        public Object mediaClip;
        public Object maskClip;

        public MaskInfo[] maskInfo;

        public Texture2D Texture { get { return (mediaClip as Texture2D); } }
        public Texture2D MaskTexture { get { return (maskClip as Texture2D); } }
        public VideoClip Video { get { return (mediaClip as VideoClip); } }
        public VideoClip MaskVideo { get { return (maskClip as VideoClip); } }

        public bool IsValid { get { return (mediaClip is Texture2D) || (mediaClip is VideoClip); } }
        public bool IsTexture { get { return (mediaClip is Texture2D); } }
        public bool IsVideo { get { return (mediaClip is VideoClip); } }

        public bool IsMaskValid { get { return (maskClip is Texture2D) || (maskClip is VideoClip); } }
        public bool IsMaskTexture { get { return (maskClip is Texture2D); } }
        public bool IsMaskVideo { get { return (maskClip is VideoClip); } }

        public bool FindMaskInfo(Color inputColor, out MaskInfo info)
        {
            for(int i = 0; i < maskInfo.Length; i++)
            {
                Debug.LogFormat("Color difference: {0} <> {1} = {2}", inputColor, maskInfo[i].colorKey, Vector4.Distance((Vector4)inputColor, (Vector4)maskInfo[i].colorKey));
                Vector4 inputColorV = inputColor;
                Vector4 maskColorV = maskInfo[i].colorKey;

                if(Vector3.Distance(inputColorV, maskColorV) < 0.1f)
                {
                    info = maskInfo[i];
                    return true;
                }
            }

            info = new MaskInfo();
            return false;
        }
    }

    [System.Serializable]
    public struct MaskInfo
    {
        public string label;
        public Color colorKey;
    }

    // ---Componets ---
    [Header("Components")]
    public VideoPlayer videoPlayer;
    public Renderer sphereRenderer;

    [Header("Mask Feature Components")]
    public MobileCameraController cameraController;
    public Collider maskSphereCollider;
    public Renderer maskSphereRenderer;
    public VideoPlayer maskVideoPlayer;

    public UnityEngine.UI.Image pointer;
    public TextMeshPro maskLabel;

    [Header("Elements")]
    public MediaElement[] mediaClips;

    // --- Variables ---
    private int index = -1;

    private Material renderMaterial { get { return sphereRenderer.material; } set { sphereRenderer.material = value; } }
    private Material maskRenderMaterial { get { return maskSphereRenderer.material; } set { maskSphereRenderer.material = value; } }

    #endregion

    private void Start()
    {
        renderMaterial = new Material(renderMaterial);
        maskRenderMaterial = new Material(maskRenderMaterial);
        MoveForward();
    }

    #region RenderMode

    public void ToogleXR()
    {
        if(XRSettings.enabled)
            DisableXR();
        else
            EnableCardboard();
    }

    public void EnableCardboard()
    {
        XRSettings.enabled = true;
        StartCoroutine(LoadDevice("Cardboard"));
    }

    public void DisableXR()
    {
        XRSettings.enabled = false;
    }

    private IEnumerator LoadDevice(string newDevice)
    {
        if(System.String.Compare(XRSettings.loadedDeviceName, newDevice, true) != 0)
        {
            XRSettings.LoadDeviceByName(newDevice);
            yield return null;
            XRSettings.enabled = true;
        }
    }

    #endregion

    #region Show Media

    public void MoveForward()
    {
        int _index = this.index + 1 < mediaClips.Length ? this.index + 1 : 0;
        ShowMedia(_index);
    }

    public void MoveBackward()
    {
        int _index = this.index - 1 > -1 ? this.index - 1 : mediaClips.Length - 1;
        ShowMedia(_index);
    }

    public void ShowMedia(int index)
    {
        if(mediaClips[index].IsTexture)
            ShowImage(index);
        else if(mediaClips[index].IsVideo)
            ShowVideo(index);

        if(mediaClips[index].IsMaskValid)
            SetMask(index);
        else
            RestMask();
    }

    public void ShowImage(int index)
    {
        renderMaterial.mainTexture = mediaClips[index].Texture;
        videoPlayer.enabled = false;
        this.index = index;
    }

    public void ShowVideo(int index)
    {
        videoPlayer.clip = mediaClips[index].Video;
        videoPlayer.enabled = true;
        this.index = index;
    }

    private void SetMask(int index)
    {
        if(mediaClips[index].IsMaskTexture)
        {
            maskRenderMaterial.mainTexture = mediaClips[index].MaskTexture;
            maskVideoPlayer.enabled = false;
            maskSphereCollider.enabled = true;
        } 
        else if(mediaClips[index].IsMaskVideo)
        {
            maskVideoPlayer.clip = mediaClips[index].Video;
            maskVideoPlayer.enabled = true;
            maskSphereCollider.enabled = true;
        }

        pointer.enabled = true;
    }

    private void RestMask()
    {
        maskRenderMaterial.mainTexture = null;
        maskVideoPlayer.clip = null;
        maskVideoPlayer.enabled = true;
        maskSphereCollider.enabled = false;
        pointer.enabled = false;
    }

    #endregion

    #region Application Controls

    public void CallExitCommand()
    {
        if(XRSettings.enabled)
            DisableXR();
        else
            Application.Quit();
    }

    #endregion

    // Update is called once per frame
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            CallExitCommand();
    }

    private void FixedUpdate()
    {
        if(!maskSphereCollider.enabled)
            return;

        RaycastHit hit;
        float length = 100f;
        Debug.DrawRay(cameraController.transform.position, cameraController.transform.forward * length, Color.red, 10f);
        if(Physics.Raycast(cameraController.transform.position, cameraController.transform.forward, out hit, length, LayerMask.GetMask("MediaClipMask")))
        {
            //Debug.LogFormat("'{0}' - Coords: {1}/{2}", hit.transform.name, hit.textureCoord.x, hit.textureCoord.y);
            Texture2D maskTexture = (maskRenderMaterial.mainTexture as Texture2D);
            Vector2 textureCoord = hit.textureCoord;
            Vector2 pixelCoord = textureCoord * new Vector2(maskTexture.width, maskTexture.height);
            Color pixelColor = maskTexture.GetPixel((int)pixelCoord.x, (int)pixelCoord.y);

            MaskInfo maskInfo;
            if(mediaClips[index].FindMaskInfo(pixelColor, out maskInfo))
            {
                //Debug.LogFormat("'{0}': {1}", maskInfo.label, pixelColor);
                maskLabel.transform.position = cameraController.transform.position + (cameraController.transform.forward * 2f);
                maskLabel.transform.rotation = cameraController.transform.rotation;

                maskLabel.text = maskInfo.label;
                maskLabel.enabled = true;
                pointer.enabled = false;
            }  
            else
            {
                //Debug.LogFormat("'Unknown': {0}", pixelColor);
                maskLabel.enabled = false;
                pointer.enabled = true;
            } 
        }
    }
}
