/**
 * LearnNova Premium UI — Interactions v3.0
 * Phase 12.1 — Global Layout
 */

/* ── Sidebar (mobile) ──────────────────────────────────────────── */
function openSidebar() {
  document.getElementById('sidebar')?.classList.add('open');
  document.getElementById('sidebar-overlay')?.classList.add('show');
  document.body.style.overflow = 'hidden';
}
function closeSidebar() {
  document.getElementById('sidebar')?.classList.remove('open');
  document.getElementById('sidebar-overlay')?.classList.remove('show');
  document.body.style.overflow = '';
}

/* ── Profile Dropdown ──────────────────────────────────────────── */
function initProfileDropdown() {
  const trigger = document.getElementById('profile-trigger');
  if (!trigger) return;

  trigger.addEventListener('click', (e) => {
    e.stopPropagation();
    const isOpen = trigger.classList.toggle('open');
    trigger.setAttribute('aria-expanded', isOpen);
  });

  document.addEventListener('click', () => {
    trigger.classList.remove('open');
    trigger.setAttribute('aria-expanded', 'false');
  });

  // Keyboard: close on Escape
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
      trigger.classList.remove('open');
      trigger.setAttribute('aria-expanded', 'false');
    }
  });
}

/* ── Toast Notification ────────────────────────────────────────── */
function toast(msg, type = 'default') {
  const colors = {
    default: { bg: '#2D1B0E', color: '#fff' },
    success: { bg: '#059669', color: '#fff' },
    error:   { bg: '#DC2626', color: '#fff' },
    info:    { bg: '#2563EB', color: '#fff' },
  };
  const c = colors[type] || colors.default;

  const t = document.createElement('div');
  t.textContent = msg;
  t.style.cssText = `
    position:fixed; bottom:100px; left:50%; transform:translateX(-50%) translateY(12px);
    background:${c.bg}; color:${c.color};
    padding:12px 24px; border-radius:99px;
    font-size:13.5px; font-family:Cairo,sans-serif; font-weight:600;
    z-index:9999; box-shadow:0 12px 32px rgba(0,0,0,.25);
    white-space:nowrap; letter-spacing:0;
    animation: toastIn 0.3s cubic-bezier(0,0,0.2,1) forwards;
  `;

  if (!document.getElementById('ln-toast-style')) {
    const s = document.createElement('style');
    s.id = 'ln-toast-style';
    s.textContent = `
      @keyframes toastIn { from{opacity:0;transform:translateX(-50%) translateY(12px)} to{opacity:1;transform:translateX(-50%) translateY(0)} }
      @keyframes toastOut { from{opacity:1;transform:translateX(-50%) translateY(0)} to{opacity:0;transform:translateX(-50%) translateY(12px)} }
    `;
    document.head.appendChild(s);
  }

  document.body.appendChild(t);
  setTimeout(() => {
    t.style.animation = 'toastOut 0.25s ease forwards';
    setTimeout(() => t.remove(), 260);
  }, 2800);
}

/* ── Stat Card Hover Lift ──────────────────────────────────────── */
function initStatCards() {
  document.querySelectorAll('.stat-card.clickable').forEach(card => {
    card.addEventListener('mouseenter', () => {
      card.style.willChange = 'transform';
    });
    card.addEventListener('mouseleave', () => {
      card.style.willChange = '';
    });
  });
}

/* ── Auto-dismiss alerts ───────────────────────────────────────── */
function initAlerts() {
  document.querySelectorAll('.alert').forEach(alert => {
    if (alert.closest('.alert-permanent')) return;
    setTimeout(() => {
      alert.style.transition = 'opacity 0.4s ease, transform 0.4s ease, max-height 0.4s ease, margin 0.4s ease, padding 0.4s ease';
      alert.style.opacity = '0';
      alert.style.transform = 'translateY(-6px)';
      setTimeout(() => alert.remove(), 420);
    }, 5000);
  });
}

/* ── Close sidebar on outside click (mobile) ────────────────────── */
function initSidebarClose() {
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeSidebar();
  });
}

/* ── Initialize all ─────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
  initProfileDropdown();
  initStatCards();
  initAlerts();
  initSidebarClose();
});
