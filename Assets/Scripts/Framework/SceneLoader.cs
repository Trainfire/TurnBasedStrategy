using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

namespace Framework
{
    public class SceneLoader : MonoBehaviour
    {
        public event Action<float> LoadProgress;

        public string LoadingScene { get; set; }

        public IEnumerator Load(string[] targetUnloadScenes, string[] targetLoadScenes, Action onLoadComplete)
        {
            if (!string.IsNullOrEmpty(LoadingScene))
                SceneManager.LoadScene(LoadingScene, LoadSceneMode.Additive);

            // Wait for the end of the current frame to finish to ensure that everything has cleaned up.
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < targetUnloadScenes.Length; i++)
            {
                SceneManager.UnloadSceneAsync(targetUnloadScenes[i]);
            }

            int scenesLoaded = 0;
            float totalProgress = 0f;
            foreach (var scene in targetLoadScenes)
            {
                var task = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

                while (!task.isDone)
                {
                    totalProgress = (scenesLoaded + task.progress) / targetLoadScenes.Length;
                    LoadProgress.InvokeSafe(totalProgress);
                    yield return null;
                }

                scenesLoaded++;
            }

            SceneManager.UnloadSceneAsync(LoadingScene);

            // We're going to assume that the last scene is what we want to be active...
            var activeScene = SceneManager.GetSceneByName(targetLoadScenes[targetLoadScenes.Length - 1]);
            SceneManager.SetActiveScene(activeScene);

            onLoadComplete.InvokeSafe();
        }
    }
}
