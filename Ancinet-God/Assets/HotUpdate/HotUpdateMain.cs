using HybridCLR;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;
using cfg;
using cfg.item;
using YooAsset;

public class HotUpdateMain : MonoBehaviour
{

    public string text;

    void Start()
    {
        Debug.Log($"[{GetType().FullName}] 这个热更新脚本挂载在prefab上，打包成ab。通过从ab中实例化prefab成功还原");
        Debug.Log($"[{GetType().FullName}] hello, {text}.");

        gameObject.AddComponent<CreateByCode>();

        Debug.Log($"[{GetType().FullName}] =======看到此条日志代表你成功运行了示例项目的热更新代码=======");


        Tables table = new Tables(Loader);
        Item it = table.TbItem.Get(10000);
        Debug.Log($"[{GetType().FullName}] 测试读表是否正常,读取商品id是10000的商品名：{it.Name}");
      
      
    }

    private void Handle_Completed(AssetOperationHandle obj)
    {

       
    }

    private JSONNode Loader(string fileName)
    {
        return JSON.Parse(File.ReadAllText(Application.streamingAssetsPath + "/GenerateDatas/json/" + fileName + ".Json"));
    }


}
