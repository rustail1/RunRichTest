#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RunRich.EditorTools
{
    /// <summary>
    /// One-time project handoff helper. After this final Assets package is imported it rebuilds
    /// Gameplay_Clean once, then writes a marker so normal editor reloads never rebuild the scene.
    /// The manual FINALIZE COMPLETE SCENE menu item remains available as a fallback.
    /// </summary>
    [InitializeOnLoad]
    public static class RunRichFinalAutoSetup
    {
        private const string MarkerPath = "Assets/_Game/Generated/CleanBuilder/RUNRICH_FINAL_SCENE_READY_V14.txt";
        private const string SessionKey = "RunRich.FinalAutoSetup.Attempting.V14";

        static RunRichFinalAutoSetup()
        {
            EditorApplication.delayCall += TryFinalize;
        }

        private static void TryFinalize()
        {
            if (File.Exists(MarkerPath))
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += TryFinalize;
                return;
            }

            if (SessionState.GetBool(SessionKey, false))
                return;

            SessionState.SetBool(SessionKey, true);
            try
            {
                RunRichCleanLevelBuilder.FinalizeCompleteScene();
                var directory = Path.GetDirectoryName(MarkerPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(
                    MarkerPath,
                    "Run Rich V14 final scene auto-setup completed after strict rebuild + validation. Delete this file and use Tools > Run Rich > FINALIZE COMPLETE SCENE to rebuild again.\n");
                AssetDatabase.ImportAsset(MarkerPath, ImportAssetOptions.ForceUpdate);
                Debug.Log("RUN RICH FINAL AUTO SETUP: Gameplay_Clean is ready.");
            }
            catch (Exception exception)
            {
                SessionState.SetBool(SessionKey, false);
                Debug.LogException(exception);
                Debug.LogWarning("RUN RICH FINAL AUTO SETUP: automatic rebuild failed. Use Tools > Run Rich > FINALIZE COMPLETE SCENE after Unity finishes importing.");
            }
        }

        [MenuItem("Tools/Run Rich/Reset Final Auto Setup Marker")]
        private static void ResetMarker()
        {
            if (File.Exists(MarkerPath))
                File.Delete(MarkerPath);
            AssetDatabase.Refresh();
            SessionState.SetBool(SessionKey, false);
            Debug.Log("RUN RICH FINAL AUTO SETUP: marker reset. Use FINALIZE COMPLETE SCENE to rebuild immediately.");
        }
    }
}
#endif
