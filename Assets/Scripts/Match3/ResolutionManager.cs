using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Match3
{
    public class ResolutionManager : MonoBehaviour
    {
        [Tooltip("Use this to adjust custom portrait(9:16) resolution on windows/mac/linux standalone builds")]
        [Range(40, 64)]
        public int m_screenResolutionMultiplier = 40;

#if !UNITY_EDITOR && UNITY_STANDALONE
        private int _newScreenWidth;
        private int _newScreenHeight;
#endif

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN

        #region Function to get the window handle
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        #endregion

        #region Function to change window style
        private const int GWL_STYLE = -16;
        private const uint WS_POPUP = 0x80000000;
        private const uint WS_BORDER = 0x00800000;

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        #endregion


        #region Function to change window position
        private const int SWP_SHOWWINDOW = 0x0040;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        #endregion
#endif


        private void Awake()
        {
            // Setup portrait resolution
            StartCoroutine(SetNewResolution());

        }

        private IEnumerator SetNewResolution()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE
            _newScreenWidth = 9 * m_screenResolutionMultiplier;
            _newScreenHeight = 16 * m_screenResolutionMultiplier;
            Screen.fullScreen = false;
            yield return new WaitForSeconds(0.0001f);
            SetNewResolutionInternal();
#else
            yield return null;
#endif
        }

        private void SetNewResolutionInternal()
        {
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            var windowHandle = GetForegroundWindow();
            Assert.IsFalse(windowHandle == IntPtr.Zero, "Failed to get window handle");

            {
                var result = SetWindowLongPtr(windowHandle, GWL_STYLE, (IntPtr)WS_BORDER);

                Assert.IsFalse(result == IntPtr.Zero, "Failed to change window style");
            }
            {
                var newScreenPosX = (Display.main.systemWidth - _newScreenWidth) * 0.5f;
                var newScreenPosY = (Display.main.systemHeight - _newScreenHeight) * 0.5f;
                var result = SetWindowPos(windowHandle, IntPtr.Zero, (int)newScreenPosX, (int)newScreenPosY, _newScreenWidth, _newScreenHeight, SWP_SHOWWINDOW);

                Assert.IsTrue(result, "Failed to move window");
            }
#elif !UNITY_EDITOR && (UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX)
            Screen.SetResolution(_newScreenWidth, _newScreenHeight, false);
#endif
        }
    }
}
