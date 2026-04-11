using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;

[InitializeOnLoad]
public class DependencyAutoInstaller
{
    static DependencyAutoInstaller()
    {
        // Jalankan pengecekan secara delay sedikit agar Unity siap
        EditorApplication.delayCall += CheckAndInstallDependencies;
    }

    private static void CheckAndInstallDependencies()
    {
        bool unitaskInstalled = IsPackageInstalled("com.cysharp.unitask");
        bool vcontainerInstalled = IsPackageInstalled("jp.hadashikick.vcontainer");

        if (!unitaskInstalled)
        {
            Debug.Log("[AudioService] Installing UniTask...");
            Client.Add("https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask");
        }

        if (!vcontainerInstalled)
        {
            Debug.Log("[AudioService] Installing VContainer...");
            Client.Add("https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0");
        }
    }

    private static bool IsPackageInstalled(string packageId)
    {
        // Mencari di daftar package yang sudah terdaftar di project
        return UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages()
               .Any(p => p.name == packageId);
    }
}