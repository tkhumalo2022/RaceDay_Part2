(() => {
  const tabs = [...document.querySelectorAll('[role="tab"]')];
  const activate = (selected, focus = false) => {
    tabs.forEach(tab => {
      const on = tab === selected;
      tab.setAttribute('aria-selected', String(on));
      tab.tabIndex = on ? 0 : -1;
      document.getElementById(tab.getAttribute('aria-controls')).hidden = !on;
    });
    if (focus) selected.focus();
  };
  tabs.forEach((tab,index)=>{
    tab.addEventListener('click',()=>activate(tab));
    tab.addEventListener('keydown',event=>{
      if (!['ArrowLeft','ArrowRight','Home','End'].includes(event.key)) return;
      event.preventDefault();
      const next=event.key==='Home'?0:event.key==='End'?tabs.length-1:(index+(event.key==='ArrowLeft'?-1:1)+tabs.length)%tabs.length;
      activate(tabs[next],true);
    });
  });
  const dialog=document.querySelector('.viewer');
  const dialogImg=dialog.querySelector('img');
  const caption=dialog.querySelector('.viewer-caption');
  document.querySelectorAll('[data-image]').forEach(button=>{
    button.addEventListener('click',()=>{
      dialogImg.src=button.dataset.image;
      dialogImg.alt=button.dataset.caption;
      caption.textContent=button.dataset.caption;
      if (dialog.showModal) dialog.showModal();
    });
  });
  dialog.addEventListener('click',event=>{if(event.target===dialog)dialog.close()});
})();