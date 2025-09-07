using Unity.Entities;
using UnityEngine;
using System.Reflection;

namespace HelperDock {
    /// 一度だけ Dock を「強制トグル＋位置リセット」で確実に表示する補助。
    [DisableAutoCreation]
    public partial class UISystemEnsureOpen : SystemBase {
        private bool _done;
        protected override void OnCreate(){ base.OnCreate(); _done = false; Debug.Log("[HelperDock] UISystemEnsureOpen created"); }
        protected override void OnUpdate(){
            if (_done) return;
            try{
                var ui = World.GetExistingSystemManaged<HelperDock.UISystem>();
                if (ui == null) return;
                var t = ui.GetType();
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                // 初期化済み＆ゲーム中チェック（あれば）
                var fInit = t.GetField("_initialized", flags);
                if (fInit != null && fInit.FieldType == typeof(bool) && !(bool)fInit.GetValue(ui)) return;
                var fInGame = t.GetField("_inGame", flags);
                if (fInGame != null && fInGame.FieldType == typeof(bool) && !(bool)fInGame.GetValue(ui)) return;

                // 必要なバインディング取得
                var fVis = t.GetField("_bindingMainPanelVisible", flags);
                var fX   = t.GetField("_bindingMainPanelPositionX", flags);
                var fY   = t.GetField("_bindingMainPanelPositionY", flags);
                if (fVis == null || fX == null || fY == null) return;

                var vis = fVis.GetValue(ui);
                var bx  = fX.GetValue(ui);
                var by  = fY.GetValue(ui);
                if (vis == null || bx == null || by == null) return;

                // Update(bool/int) を反射で叩く
                var mUpdateBool = vis.GetType().GetMethod("Update", new[] { typeof(bool) });
                var mUpdateInt  = bx.GetType().GetMethod("Update", new[] { typeof(int) });
                if (mUpdateBool == null || mUpdateInt == null) return;

                // 位置を安全値へ
                mUpdateInt.Invoke(bx, new object[] { 80 });
                mUpdateInt.Invoke(by, new object[] { 120 });
                HelperDock.ModCompat.ModSettings.MainPanelPositionX = 80;
                HelperDock.ModCompat.ModSettings.MainPanelPositionY = 120;

                // false -> true に強制トグル（UIへ変化通知）
                mUpdateBool.Invoke(vis, new object[] { false });
                HelperDock.ModCompat.ModSettings.MainPanelVisible = false;
                HelperDock.ModCompat.ModSettings.ApplyAndSave();

                mUpdateBool.Invoke(vis, new object[] { true });
                HelperDock.ModCompat.ModSettings.MainPanelVisible = true;
                HelperDock.ModCompat.ModSettings.ApplyAndSave();

                // あれば UI 側のボタンハンドラを一回だけ呼ぶ
                var mMainButton = t.GetMethod("MainButtonClicked", flags);
                if (mMainButton != null) mMainButton.Invoke(ui, null);

                Debug.Log("[HelperDock] UISystemEnsureOpen: forced toggle + pos reset OK");
                _done = true;
            }catch(System.Exception ex){
                Debug.Log("[HelperDock] UISystemEnsureOpen error: " + ex.Message);
            }
        }
    }
}
