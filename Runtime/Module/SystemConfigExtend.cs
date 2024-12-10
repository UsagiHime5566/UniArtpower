using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class SystemConfigExtend
{
    public static void SetLabels(this Dropdown dd, string [] aName)
    {
        dd.options.Clear();
        foreach (var item in aName)
        {
            Dropdown.OptionData data = new Dropdown.OptionData(){
                text = item
            };
            dd.options.Add(data);
        }
    }

    public static int SetSavedData(this Dropdown dd, string savedString, Action<int> changedToDo){
        int id = SystemConfig.Instance.GetData<int>(savedString);
        dd.value = id;
        dd.captionText.text = dd.options[dd.value].text;

        dd.onValueChanged.AddListener(x => {
            SystemConfig.Instance.SaveData(savedString, x);
            changedToDo?.Invoke(x);
        });

        //如果 id 大於 DropValue, 他會自己限縮, 因此要讀取 DropValue
        int resultId = dd.value;
        dd.onValueChanged.Invoke(resultId);

        return resultId;
    }

    public static void SetSavedDataFloat(this InputField inp, string savedString, float defValue, Action<float> changedToDo){
        float val = SystemConfig.Instance.GetData<float>(savedString, defValue);
        inp.onValueChanged.AddListener(x => {
            float result = defValue;
            if(!float.TryParse(x, out result))
                return;

            SystemConfig.Instance.SaveData(savedString, result);
            changedToDo?.Invoke(result);
        });
        inp.text = val.ToString("0.00");
    }

    public static void SetSavedDataFloat(this Slider sld, string savedString, float defValue, Action<float> changedToDo){
        float val = SystemConfig.Instance.GetData<float>(savedString, defValue);
        sld.onValueChanged.AddListener(x => {
            SystemConfig.Instance.SaveData(savedString, x);
            changedToDo?.Invoke(x);
        });
        sld.value = val;
    }
}
