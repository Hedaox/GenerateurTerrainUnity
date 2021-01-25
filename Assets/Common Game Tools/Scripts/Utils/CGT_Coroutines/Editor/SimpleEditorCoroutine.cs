using System;
using System.Collections;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CGT
{
    public class SimpleEditorCoroutine
    {
        public static SimpleEditorCoroutine Start(IEnumerator _routine)
        {
            SimpleEditorCoroutine coroutine = new SimpleEditorCoroutine(_routine);
            coroutine.Start();
            return coroutine;
        }

        readonly IEnumerator routine;
        SimpleEditorCoroutine(IEnumerator _routine)
        {
            routine = _routine;
        }

        void Start()
        {
            EditorApplication.update += Update;
        }
        public void Stop()
        {
            EditorApplication.update -= Update;
        }

        void Update()
        {
            if (!routine.MoveNext())
            {
                Stop();
            }
        }
    }
}