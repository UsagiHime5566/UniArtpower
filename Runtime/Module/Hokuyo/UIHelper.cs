using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HimeLib
{
    public class UIHelper : MonoBehaviour
    {
        public HokuyoHelper hokuyoHelper;

        public Text isLoading;

        public Toggle toggleUseEmuSignal;
        public Slider sliderSignalDelay;

        public InputField inputFieldMinX;
        public InputField inputFieldMaxX;
        public InputField inputFieldMinY;
        public InputField inputFieldMaxY;

        public InputField laserRange;
        public InputField groupRange;

        public InputField UstIndexStart;
        public InputField UstIndexEnd;
        public InputField UstDegree;

        public Button buttonUpUstDegree;
        public Button buttonDownUstDegree;

        public List<InputField> sensorCorners;
        public List<InputField> worldCorners;

        public Text textOutput;

        void Start()
        {
            hokuyoHelper.OnScreenPosCome += v2 => {
                textOutput.text = v2.ToString();
            };

            hokuyoHelper.readEmuSignal.OnReadEmuSignalReady += () => {
                isLoading.enabled = false;
            };

            hokuyoHelper.useEmuSignal = CustomSaveLoad.Instance.GetData<bool>("useEmuSignal", true);
            hokuyoHelper.readEmuSignal.signalDelay = CustomSaveLoad.Instance.GetData<float>("signalDelay", hokuyoHelper.readEmuSignal.signalDelay);
            hokuyoHelper.rawLocationFilter_X.min = CustomSaveLoad.Instance.GetData<float>("rawLocationFilter_X_min", -2);
            hokuyoHelper.rawLocationFilter_X.max = CustomSaveLoad.Instance.GetData<float>("rawLocationFilter_X_max", 2);
            hokuyoHelper.rawLocationFilter_Y.min = CustomSaveLoad.Instance.GetData<float>("rawLocationFilter_Y_min", -2);
            hokuyoHelper.rawLocationFilter_Y.max = CustomSaveLoad.Instance.GetData<float>("rawLocationFilter_Y_max", 2);
            hokuyoHelper.distanceFilter = CustomSaveLoad.Instance.GetData<float>("distanceFilter", hokuyoHelper.distanceFilter);
            hokuyoHelper.euclidianCluster = CustomSaveLoad.Instance.GetData<float>("euclidianCluster", hokuyoHelper.euclidianCluster);
            hokuyoHelper.urg.startIndex = CustomSaveLoad.Instance.GetData<int>("urg_startIndex", hokuyoHelper.urg.startIndex);
            hokuyoHelper.urg.endIndex = CustomSaveLoad.Instance.GetData<int>("urg_endIndex", hokuyoHelper.urg.endIndex);
            hokuyoHelper.urg.offsetDegrees = CustomSaveLoad.Instance.GetData<float>("urg_offsetDegrees", hokuyoHelper.urg.offsetDegrees);
            hokuyoHelper.sensorCorners[3].x = CustomSaveLoad.Instance.GetData<float>("sensorCorners_3_x", hokuyoHelper.sensorCorners[3].x);
            hokuyoHelper.sensorCorners[3].y = CustomSaveLoad.Instance.GetData<float>("sensorCorners_3_y", hokuyoHelper.sensorCorners[3].y);
            hokuyoHelper.sensorCorners[0].x = CustomSaveLoad.Instance.GetData<float>("sensorCorners_0_x", hokuyoHelper.sensorCorners[0].x);
            hokuyoHelper.sensorCorners[0].y = CustomSaveLoad.Instance.GetData<float>("sensorCorners_0_y", hokuyoHelper.sensorCorners[0].y);
            hokuyoHelper.sensorCorners[2].x = CustomSaveLoad.Instance.GetData<float>("sensorCorners_2_x", hokuyoHelper.sensorCorners[2].x);
            hokuyoHelper.sensorCorners[2].y = CustomSaveLoad.Instance.GetData<float>("sensorCorners_2_y", hokuyoHelper.sensorCorners[2].y);
            hokuyoHelper.sensorCorners[1].x = CustomSaveLoad.Instance.GetData<float>("sensorCorners_1_x", hokuyoHelper.sensorCorners[1].x);
            hokuyoHelper.sensorCorners[1].y = CustomSaveLoad.Instance.GetData<float>("sensorCorners_1_y", hokuyoHelper.sensorCorners[1].y);
            hokuyoHelper.worldCorners[3].x = CustomSaveLoad.Instance.GetData<float>("worldCorners_3_x", hokuyoHelper.worldCorners[3].x);
            hokuyoHelper.worldCorners[3].y = CustomSaveLoad.Instance.GetData<float>("worldCorners_3_y", hokuyoHelper.worldCorners[3].y);
            hokuyoHelper.worldCorners[0].x = CustomSaveLoad.Instance.GetData<float>("worldCorners_0_x", hokuyoHelper.worldCorners[0].x);
            hokuyoHelper.worldCorners[0].y = CustomSaveLoad.Instance.GetData<float>("worldCorners_0_y", hokuyoHelper.worldCorners[0].y);
            hokuyoHelper.worldCorners[2].x = CustomSaveLoad.Instance.GetData<float>("worldCorners_2_x", hokuyoHelper.worldCorners[2].x);
            hokuyoHelper.worldCorners[2].y = CustomSaveLoad.Instance.GetData<float>("worldCorners_2_y", hokuyoHelper.worldCorners[2].y);
            hokuyoHelper.worldCorners[1].x = CustomSaveLoad.Instance.GetData<float>("worldCorners_1_x", hokuyoHelper.worldCorners[1].x);
            hokuyoHelper.worldCorners[1].y = CustomSaveLoad.Instance.GetData<float>("worldCorners_1_y", hokuyoHelper.worldCorners[1].y);

            hokuyoHelper.SetupUrgFilters();
            hokuyoHelper.SetupAffineConverter();
            isLoading.gameObject.SetActive(hokuyoHelper.useEmuSignal);

            toggleUseEmuSignal.isOn = hokuyoHelper.useEmuSignal;
            toggleUseEmuSignal.onValueChanged.AddListener(x => {
                hokuyoHelper.useEmuSignal = x;
                isLoading.gameObject.SetActive(x);
                CustomSaveLoad.Instance.SaveData("useEmuSignal", x);
            });

            sliderSignalDelay.value = hokuyoHelper.readEmuSignal.signalDelay;
            sliderSignalDelay.onValueChanged.AddListener(x => {
                hokuyoHelper.readEmuSignal.signalDelay = x;
                CustomSaveLoad.Instance.SaveData("signalDelay", x);
            });

            inputFieldMinX.text = hokuyoHelper.rawLocationFilter_X.min.ToString();
            inputFieldMaxX.text = hokuyoHelper.rawLocationFilter_X.max.ToString();
            inputFieldMinY.text = hokuyoHelper.rawLocationFilter_Y.min.ToString();
            inputFieldMaxY.text = hokuyoHelper.rawLocationFilter_Y.max.ToString();

            inputFieldMinX.onValueChanged.AddListener(x => {
                hokuyoHelper.rawLocationFilter_X.min = float.Parse(x);
                CustomSaveLoad.Instance.SaveData("rawLocationFilter_X_min", hokuyoHelper.rawLocationFilter_X.min);
            });
            inputFieldMaxX.onValueChanged.AddListener(x => {
                hokuyoHelper.rawLocationFilter_X.max = float.Parse(x);
                CustomSaveLoad.Instance.SaveData("rawLocationFilter_X_max", hokuyoHelper.rawLocationFilter_X.max);
            });
            inputFieldMinY.onValueChanged.AddListener(x => {
                hokuyoHelper.rawLocationFilter_Y.min = float.Parse(x);
                CustomSaveLoad.Instance.SaveData("rawLocationFilter_Y_min", hokuyoHelper.rawLocationFilter_Y.min);
            });
            inputFieldMaxY.onValueChanged.AddListener(x => {
                hokuyoHelper.rawLocationFilter_Y.max = float.Parse(x);
                CustomSaveLoad.Instance.SaveData("rawLocationFilter_Y_max", hokuyoHelper.rawLocationFilter_Y.max);
            });

            laserRange.text = hokuyoHelper.distanceFilter.ToString();
            laserRange.onValueChanged.AddListener(x => {
                hokuyoHelper.distanceFilter = float.Parse(x);
                hokuyoHelper.SetupUrgFilters();
                CustomSaveLoad.Instance.SaveData("distanceFilter", hokuyoHelper.distanceFilter);
            });

            groupRange.text = hokuyoHelper.euclidianCluster.ToString();
            groupRange.onValueChanged.AddListener(x => {
                hokuyoHelper.euclidianCluster = float.Parse(x);
                hokuyoHelper.SetupUrgFilters();
                CustomSaveLoad.Instance.SaveData("euclidianCluster", hokuyoHelper.euclidianCluster);
            });

            UstIndexStart.text = hokuyoHelper.urg.startIndex.ToString();
            UstIndexStart.onValueChanged.AddListener(x => {
                hokuyoHelper.urg.startIndex = int.Parse(x);
                CustomSaveLoad.Instance.SaveData("urg_startIndex", hokuyoHelper.urg.startIndex);
            });
            UstIndexEnd.text = hokuyoHelper.urg.endIndex.ToString();
            UstIndexEnd.onValueChanged.AddListener(x => {
                hokuyoHelper.urg.endIndex = int.Parse(x);
                CustomSaveLoad.Instance.SaveData("urg_endIndex", hokuyoHelper.urg.endIndex);
            });
            UstDegree.text = hokuyoHelper.urg.offsetDegrees.ToString();
            UstDegree.onValueChanged.AddListener(x => {
                hokuyoHelper.urg.offsetDegrees = float.Parse(x);
                CustomSaveLoad.Instance.SaveData("urg_offsetDegrees", hokuyoHelper.urg.offsetDegrees);
            });

            buttonUpUstDegree.onClick.AddListener(() => {
                hokuyoHelper.urg.offsetDegrees += 5;
                UstDegree.SetTextWithoutNotify(hokuyoHelper.urg.offsetDegrees.ToString());
                CustomSaveLoad.Instance.SaveData("urg_offsetDegrees", hokuyoHelper.urg.offsetDegrees);
            });
            buttonDownUstDegree.onClick.AddListener(() => {
                hokuyoHelper.urg.offsetDegrees -= 5;
                UstDegree.SetTextWithoutNotify(hokuyoHelper.urg.offsetDegrees.ToString());
                CustomSaveLoad.Instance.SaveData("urg_offsetDegrees", hokuyoHelper.urg.offsetDegrees);
            });

            sensorCorners[0].text = hokuyoHelper.sensorCorners[3].x.ToString();
            sensorCorners[1].text = hokuyoHelper.sensorCorners[3].y.ToString();
            sensorCorners[2].text = hokuyoHelper.sensorCorners[0].x.ToString();
            sensorCorners[3].text = hokuyoHelper.sensorCorners[0].y.ToString();
            sensorCorners[4].text = hokuyoHelper.sensorCorners[2].x.ToString();
            sensorCorners[5].text = hokuyoHelper.sensorCorners[2].y.ToString();
            sensorCorners[6].text = hokuyoHelper.sensorCorners[1].x.ToString();
            sensorCorners[7].text = hokuyoHelper.sensorCorners[1].y.ToString();

            worldCorners[0].text = hokuyoHelper.worldCorners[3].x.ToString();
            worldCorners[1].text = hokuyoHelper.worldCorners[3].y.ToString();
            worldCorners[2].text = hokuyoHelper.worldCorners[0].x.ToString();
            worldCorners[3].text = hokuyoHelper.worldCorners[0].y.ToString();
            worldCorners[4].text = hokuyoHelper.worldCorners[2].x.ToString();
            worldCorners[5].text = hokuyoHelper.worldCorners[2].y.ToString();
            worldCorners[6].text = hokuyoHelper.worldCorners[1].x.ToString();
            worldCorners[7].text = hokuyoHelper.worldCorners[1].y.ToString();

            sensorCorners[0].onValueChanged.AddListener(x => {
                hokuyoHelper.sensorCorners[3].x = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("sensorCorners_3_x", hokuyoHelper.sensorCorners[3].x);
            });
            sensorCorners[1].onValueChanged.AddListener(x => {
                hokuyoHelper.sensorCorners[3].y = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("sensorCorners_3_y", hokuyoHelper.sensorCorners[3].y);
            });
            sensorCorners[2].onValueChanged.AddListener(x => {
                hokuyoHelper.sensorCorners[0].x = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("sensorCorners_0_x", hokuyoHelper.sensorCorners[0].x);
            });
            sensorCorners[3].onValueChanged.AddListener(x => {
                hokuyoHelper.sensorCorners[0].y = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("sensorCorners_0_y", hokuyoHelper.sensorCorners[0].y);
            });
            sensorCorners[4].onValueChanged.AddListener(x => {
                hokuyoHelper.sensorCorners[2].x = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("sensorCorners_2_x", hokuyoHelper.sensorCorners[2].x);
            });
            sensorCorners[5].onValueChanged.AddListener(x => {
                hokuyoHelper.sensorCorners[2].y = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("sensorCorners_2_y", hokuyoHelper.sensorCorners[2].y);
            });
            sensorCorners[6].onValueChanged.AddListener(x => {
                hokuyoHelper.sensorCorners[1].x = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("sensorCorners_1_x", hokuyoHelper.sensorCorners[1].x);
            });
            sensorCorners[7].onValueChanged.AddListener(x => {
                hokuyoHelper.sensorCorners[1].y = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("sensorCorners_1_y", hokuyoHelper.sensorCorners[1].y);
            });

            worldCorners[0].onValueChanged.AddListener(x => {
                hokuyoHelper.worldCorners[3].x = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("worldCorners_3_x", hokuyoHelper.worldCorners[3].x);
            });
            worldCorners[1].onValueChanged.AddListener(x => {
                hokuyoHelper.worldCorners[3].y = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("worldCorners_3_y", hokuyoHelper.worldCorners[3].y);
            });
            worldCorners[2].onValueChanged.AddListener(x => {
                hokuyoHelper.worldCorners[0].x = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("worldCorners_0_x", hokuyoHelper.worldCorners[0].x);
            });
            worldCorners[3].onValueChanged.AddListener(x => {
                hokuyoHelper.worldCorners[0].y = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("worldCorners_0_y", hokuyoHelper.worldCorners[0].y);
            });
            worldCorners[4].onValueChanged.AddListener(x => {
                hokuyoHelper.worldCorners[2].x = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("worldCorners_2_x", hokuyoHelper.worldCorners[2].x);
            });
            worldCorners[5].onValueChanged.AddListener(x => {
                hokuyoHelper.worldCorners[2].y = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("worldCorners_2_y", hokuyoHelper.worldCorners[2].y);
            });
            worldCorners[6].onValueChanged.AddListener(x => {
                hokuyoHelper.worldCorners[1].x = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("worldCorners_1_x", hokuyoHelper.worldCorners[1].x);
            });
            worldCorners[7].onValueChanged.AddListener(x => {
                hokuyoHelper.worldCorners[1].y = float.Parse(x);
                hokuyoHelper.SetupAffineConverter();
                CustomSaveLoad.Instance.SaveData("worldCorners_1_y", hokuyoHelper.worldCorners[1].y);
            });

            AutoCreateEventSystem();
        }

        public void AutoCreateEventSystem(){
            // 檢查場景中是否已存在 EventSystem
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                // 創建新的 GameObject 並添加 EventSystem 組件
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                
                Debug.Log("自動創建 EventSystem");
            }
        }
    }
}
