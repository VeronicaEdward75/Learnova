// Shared helpers ported from style/script.js — kept dependency-free and framework-agnostic
// so both _Layout.cshtml and the auth views can use them.

function toast(msg) {
  const t = document.createElement('div');
  t.textContent = msg;
  t.style.cssText = 'position:fixed;bottom:90px;left:50%;transform:translateX(-50%);background:#3E2723;color:#fff;padding:10px 20px;border-radius:999px;font-size:13px;z-index:5000;box-shadow:0 8px 24px rgba(0,0,0,.3)';
  document.body.appendChild(t);
  setTimeout(() => t.remove(), 2600);
}

function openSidebar() {
  document.getElementById('sidebar')?.classList.add('open');
  document.getElementById('sidebar-overlay')?.classList.add('show');
}

function closeSidebar() {
  document.getElementById('sidebar')?.classList.remove('open');
  document.getElementById('sidebar-overlay')?.classList.remove('show');
}
