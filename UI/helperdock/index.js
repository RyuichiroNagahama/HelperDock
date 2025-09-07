// 最小の“受け皿”。C# 側の Binding が変われば、このJSで見た目を切り替え可能。
// （Cities: Skylines II の cohtml では window.engine 経由でイベントを受ける設計が一般的）
(function(){
  const root = document.getElementById('dock');

  // 画面側の可視状態は C# 側の Binding(MainPanelVisible)で最終決定する想定。
  // ここでは最低限、ページが読めた印として表示しておく。
  root.style.display = 'block';

  // ドラッグ移動の最小実装（将来: これを C# 側に座標反映）
  let dragging=false, ox=0, oy=0;
  root.querySelector('.title').addEventListener('mousedown', (e)=>{
    dragging=true; ox=e.clientX - root.offsetLeft; oy=e.clientY - root.offsetTop;
  });
  window.addEventListener('mouseup', ()=> dragging=false);
  window.addEventListener('mousemove', (e)=>{
    if(!dragging) return;
    root.style.left = (e.clientX-ox)+'px';
    root.style.top  = (e.clientY-oy)+'px';
  });
})();
