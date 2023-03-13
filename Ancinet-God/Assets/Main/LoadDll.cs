using HybridCLR;

using System;

using System.Collections;

using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using YooAsset;



public class LoadDll : MonoBehaviour
{

    // ��Դϵͳ����ģʽ

    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;



    //CDN��ַ

    public string DefaultHostServer = "http://192.168.3.3/Package/";

    public string FallbackHostServer = "http://192.168.3.3/Package/";



    public string HotDllName = "Hotfix.dll";



    //����Ԫ����dll���б�Yooasset�в���Ҫ����׺

    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()

    {

        "mscorlib.dll",

        "System.dll",

        "System.Core.dll",

    };



    //��ȡ��Դ������

    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();

    public static byte[] GetAssetData(string dllName)

    {

        return s_assetDatas[dllName];

    }





    void Start()
    {

        //��ʼ��BetterStreamingAssets���
        BetterStreamingAssets.Initialize();

        StartCoroutine(DownLoadAssetsByYooAssets(this.StartGame));
    }





    #region Yooasset����



    IEnumerator DownLoadAssetsByYooAssets(Action onDownloadComplete)
    {
        // 1.��ʼ����Դϵͳ
        YooAssets.Initialize();



        // ����Ĭ�ϵ���Դ��
        var package = YooAssets.CreateAssetsPackage("DefaultPackage");



        // ���ø���Դ��ΪĬ�ϵ���Դ��������ʹ��YooAssets��ؼ��ؽӿڼ��ظ���Դ�����ݡ�
        YooAssets.SetDefaultAssetsPackage(package);



        if (PlayMode == EPlayMode.EditorSimulateMode)

        {

            //�༭��ģ��ģʽ

            var initParameters = new EditorSimulateModeParameters();

            initParameters.SimulatePatchManifestPath = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");

            yield return package.InitializeAsync(initParameters);

        }

        else if (PlayMode == EPlayMode.HostPlayMode)

        {

            //��������ģʽ

            var initParameters = new HostPlayModeParameters();

            initParameters.QueryServices = new QueryStreamingAssetsFileServices();

            initParameters.DefaultHostServer = DefaultHostServer;

            initParameters.FallbackHostServer = FallbackHostServer;

            yield return package.InitializeAsync(initParameters);

        }

        else if (PlayMode == EPlayMode.OfflinePlayMode)

        {

            //����ģʽ

            var initParameters = new OfflinePlayModeParameters();

            yield return package.InitializeAsync(initParameters);



        }



        //2.��ȡ��Դ�汾

        var operation = package.UpdatePackageVersionAsync();

        yield return operation;



        if (operation.Status != EOperationStatus.Succeed)

        {

            //����ʧ��

            Debug.LogError(operation.Error);

            //TODO

            yield break;

        }

        string PackageVersion = operation.PackageVersion;



        //3.���²����嵥

        var operation2 = package.UpdatePackageManifestAsync(PackageVersion);

        yield return operation2;



        if (operation2.Status != EOperationStatus.Succeed)

        {

            //����ʧ��

            Debug.LogError(operation2.Error);

            //TODO:

            yield break;

        }



        //4.���ز�����

        yield return Download();



        //TODO:�ж��Ƿ����سɹ�...

        //�ȸ���Dll����

        var Allassets = new List<string>

        {

            HotDllName,



        }.Concat(AOTMetaAssemblyNames);

        foreach (var asset in Allassets)

        {

            RawFileOperationHandle handle = package.LoadRawFileAsync(asset);

            //var handle = package.LoadAssetAsync<GameObject>(asset);

            yield return handle;

            byte[] fileData = handle.GetRawFileData();

            s_assetDatas[asset] = fileData;

            Debug.Log($"dll:{asset}  size:{fileData.Length}");



        }



        onDownloadComplete();

    }



    IEnumerator Download()

    {

        int downloadingMaxNum = 10;

        int failedTryAgain = 3;

        int timeout = 60;

        var package = YooAssets.GetAssetsPackage("DefaultPackage");

        var downloader = package.CreatePatchDownloader(downloadingMaxNum, failedTryAgain, timeout);



        //û����Ҫ���ص���Դ

        if (downloader.TotalDownloadCount == 0)

        {

            Debug.Log("û����Դ����");

            yield break;

        }



        //��Ҫ���ص��ļ��������ܴ�С

        int totalDownloadCount = downloader.TotalDownloadCount;

        long totalDownloadBytes = downloader.TotalDownloadBytes;

        Debug.Log($"�ļ�����:{totalDownloadCount}:::�ܴ�С:{totalDownloadBytes}");

        //ע��ص�����

        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;

        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;

        downloader.OnDownloadOverCallback = OnDownloadOverFunction;

        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;



        //��������

        downloader.BeginDownload();

        yield return downloader;



        //������ؽ��

        if (downloader.Status == EOperationStatus.Succeed)

        {

            //���سɹ�

            Debug.Log("�������!");

            //TODO:

        }

        else

        {

            //����ʧ��

            Debug.LogError("����ʧ�ܣ�");

            //TODO:

        }

    }



    /// <summary>

    /// ��ʼ����

    /// </summary>

    /// <param name="fileName"></param>

    /// <param name="sizeBytes"></param>

    /// <exception cref="NotImplementedException"></exception>

    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)

    {

        Debug.Log(string.Format("��ʼ���أ��ļ�����{0}, �ļ���С��{1}", fileName, sizeBytes));

    }



    /// <summary>

    /// �������

    /// </summary>

    /// <param name="isSucceed"></param>

    /// <exception cref="NotImplementedException"></exception>

    private void OnDownloadOverFunction(bool isSucceed)

    {

        Debug.Log("����" + (isSucceed ? "�ɹ�" : "ʧ��"));

    }



    /// <summary>

    /// ������

    /// </summary>

    /// <param name="totalDownloadCount"></param>

    /// <param name="currentDownloadCount"></param>

    /// <param name="totalDownloadBytes"></param>

    /// <param name="currentDownloadBytes"></param>

    /// <exception cref="NotImplementedException"></exception>

    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)

    {

        Debug.Log(string.Format("�ļ�������{0}, �������ļ�����{1}, �����ܴ�С��{2}, �����ش�С��{3}", totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes));

    }



    /// <summary>

    /// ���س���

    /// </summary>

    /// <param name="fileName"></param>

    /// <param name="error"></param>

    /// <exception cref="NotImplementedException"></exception>

    private void OnDownloadErrorFunction(string fileName, string error)

    {

        Debug.LogError(string.Format("���س����ļ�����{0}, ������Ϣ��{1}", fileName, error));

    }



    // �����ļ���ѯ������

    private class QueryStreamingAssetsFileServices : IQueryServices

    {

        public bool QueryStreamingAssets(string fileName)

        {

            // ע�⣺ʹ����BetterStreamingAssets�����ʹ��ǰ��Ҫ��ʼ���ò����

            string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();

            return BetterStreamingAssets.FileExists($"{buildinFolderName}/{fileName}");

        }

    }



    #endregion



    void StartGame()

    {

        LoadMetadataForAOTAssemblies();



#if !UNITY_EDITOR

        System.Reflection.Assembly.Load(GetAssetData("Hotfix.dll"));

#endif

        //ί�м��ط�ʽ������prefab

        var package = YooAssets.GetAssetsPackage("DefaultPackage");

        AssetOperationHandle handle = package.LoadAssetAsync<GameObject>("HotUpdatePrefab");

        handle.Completed += Handle_Completed;

    }

    private void Handle_Completed(AssetOperationHandle obj)

    {

        GameObject go = obj.InstantiateSync();

        Debug.Log($"Prefab name is {go.name}");

    }





    /// <summary>

    /// Ϊaot assembly����ԭʼmetadata�� ��������aot�����ȸ��¶��С�

    /// һ�����غ����AOT���ͺ�����Ӧnativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��

    /// </summary>

    private static void LoadMetadataForAOTAssemblies()

    {

        /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�

        /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���

        HomologousImageMode mode = HomologousImageMode.SuperSet;

        foreach (var aotDllName in AOTMetaAssemblyNames)

        {

            byte[] dllBytes = GetAssetData(aotDllName);

            // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����

            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);

            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");

        }

    }

}