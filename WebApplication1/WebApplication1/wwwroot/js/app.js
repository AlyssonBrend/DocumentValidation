// ── State ─────────────────────────────────────────────────────────────────────
const state = {
  lang: localStorage.getItem('lang') || 'en',
  token: localStorage.getItem('token') || null,
  employeeName: localStorage.getItem('employeeName') || null,
  currentPage: 'home'
};

// ── i18n ──────────────────────────────────────────────────────────────────────
function t(key) {
  const keys = key.split('.');
  let val = translations[state.lang];
  for (const k of keys) val = val?.[k];
  return val ?? key;
}

function applyTranslations() {
  document.querySelectorAll('[data-i18n]').forEach(el => {
    el.textContent = t(el.dataset.i18n);
  });
  document.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
    el.placeholder = t(el.dataset.i18nPlaceholder);
  });
  // Repopulate country and doctype selects with translated options
  buildCountrySelect();
  buildDocTypeSelect();
}

function setLang(lang) {
  state.lang = lang;
  localStorage.setItem('lang', lang);
  document.getElementById('langSelect').value = lang;
  applyTranslations();
}

// ── Document input config ─────────────────────────────────────────────────────
const docConfig = {
  CPF:        { mask: '###.###.###-##', numeric: true,  icon: '🪪', hint: 'CPF: 000.000.000-00',         maxRaw: 11,  isMrz: false },
  CNH:        { mask: '###########',    numeric: true,  icon: '🚗', hint: 'CNH: 11 dígitos numéricos',    maxRaw: 11,  isMrz: false },
  Passport:   { mask: null,             numeric: false, icon: '📘', hint: 'MRZ — 2 linhas de 44 chars',  maxRaw: 90,  isMrz: true  },
  EuropeanId: { mask: null,             numeric: false, icon: '🪪', hint: 'Número alfanumérico do documento', maxRaw: 30, isMrz: false },
};

function applyMask(raw, mask) {
  let out = '';
  let ri = 0;
  for (let i = 0; i < mask.length && ri < raw.length; i++) {
    out += mask[i] === '#' ? raw[ri++] : mask[i];
  }
  return out;
}

function onDocValueInput(e) {
  const type = document.getElementById('docTypeSelect').value;
  const cfg = docConfig[type];
  if (!cfg?.mask) return;

  const cursor = e.target.selectionStart;
  const rawBefore = e.target.value.slice(0, cursor).replace(/\D/g, '').length;
  const raw = e.target.value.replace(/\D/g, '').slice(0, cfg.maxRaw);
  const masked = applyMask(raw, cfg.mask);
  e.target.value = masked;

  // Restore cursor: advance past mask separators
  let pos = 0, count = 0;
  while (pos < masked.length && count < rawBefore) {
    if (cfg.mask[pos] === '#') count++;
    pos++;
  }
  e.target.setSelectionRange(pos, pos);
}

function updateDocInput() {
  const type = document.getElementById('docTypeSelect').value;
  const cfg = docConfig[type] ?? { numeric: false, icon: '📄', hint: '', isMrz: false, maxRaw: 200 };

  const inputEl  = document.getElementById('docValue');
  const mrzEl    = document.getElementById('docValueMrz');
  const iconEl   = document.getElementById('docInputIcon');
  const hintEl   = document.getElementById('docInputHint');

  iconEl.textContent = cfg.icon;
  hintEl.textContent = cfg.hint;

  if (cfg.isMrz) {
    inputEl.classList.add('hidden');
    mrzEl.classList.remove('hidden');
    mrzEl.value = '';
  } else {
    mrzEl.classList.add('hidden');
    inputEl.classList.remove('hidden');
    inputEl.value = '';
    inputEl.inputMode = cfg.numeric ? 'numeric' : 'text';
    inputEl.placeholder = cfg.mask ? cfg.mask.replace(/#/g, '0') : cfg.hint;
    inputEl.setAttribute('data-mask', cfg.mask || '');
    inputEl.setAttribute('data-numeric', cfg.numeric ? '1' : '');
  }
}

// ── Country / DocType selects ─────────────────────────────────────────────────
const countryDocTypes = {
  BR:   ['CPF', 'CNH', 'Passport'],
  DE:   ['EuropeanId', 'Passport'],
  FR:   ['EuropeanId', 'Passport'],
  PT:   ['EuropeanId', 'Passport'],
  ES:   ['EuropeanId', 'Passport'],
  IT:   ['EuropeanId', 'Passport'],
  NL:   ['EuropeanId', 'Passport'],
  INTL: ['Passport', 'EuropeanId']
};

function buildCountrySelect() {
  const sel = document.getElementById('countrySelect');
  const current = sel.value;
  sel.innerHTML = '';
  for (const [code, name] of Object.entries(t('countries'))) {
    const opt = document.createElement('option');
    opt.value = code;
    opt.textContent = name;
    sel.appendChild(opt);
  }
  if (current) sel.value = current;
  buildDocTypeSelect();
}

function buildDocTypeSelect() {
  const country = document.getElementById('countrySelect').value || 'INTL';
  const sel = document.getElementById('docTypeSelect');
  const types = countryDocTypes[country] || Object.keys(t('docTypes'));
  const allTypes = t('docTypes');
  sel.innerHTML = '';
  for (const type of types) {
    const opt = document.createElement('option');
    opt.value = type;
    opt.textContent = allTypes[type] ?? type;
    sel.appendChild(opt);
  }
  updateDocInput();
}

// ── Navigation ────────────────────────────────────────────────────────────────
function showPage(pageId) {
  document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
  document.getElementById('page-' + pageId)?.classList.add('active');
  state.currentPage = pageId;

  if (pageId === 'documents') loadDocuments();
  if (pageId === 'clients') loadClients();
  if (pageId === 'worker') renderWorkerArea();
}

// ── Validation ────────────────────────────────────────────────────────────────
document.getElementById('validateForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  const country = document.getElementById('countrySelect').value;
  const type    = document.getElementById('docTypeSelect').value;
  const cfg     = docConfig[type];

  const rawEl = cfg?.isMrz ? document.getElementById('docValueMrz') : document.getElementById('docValue');
  let value   = rawEl.value.trim();

  // Strip mask separators for numeric documents before sending to API
  if (cfg?.numeric) value = value.replace(/\D/g, '');

  if (!value) return;

  const btn = document.getElementById('btnValidate');
  btn.disabled = true;

  try {
    const headers = { 'Content-Type': 'application/json' };
    if (state.token) headers['Authorization'] = 'Bearer ' + state.token;

    const res = await fetch('/api/validation', {
      method: 'POST',
      headers,
      body: JSON.stringify({ type, value, country })
    });

    const data = await res.json();
    renderResult(data);
  } catch {
    renderError();
  } finally {
    btn.disabled = false;
  }
});

function renderResult(data) {
  const box = document.getElementById('resultBox');
  box.classList.remove('hidden', 'result-valid', 'result-invalid');
  box.classList.add(data.isValid ? 'result-valid' : 'result-invalid');

  document.getElementById('resultBadge').textContent = data.isValid ? t('resultValid') : t('resultInvalid');
  document.getElementById('resultScore').textContent = `${t('resultScore')}: ${data.confidenceScore}%`;

  const errList = document.getElementById('resultErrors');
  errList.innerHTML = '';
  (data.errors || []).forEach(e => {
    const li = document.createElement('li');
    li.textContent = e;
    errList.appendChild(li);
  });
  document.getElementById('errorsSection').classList.toggle('hidden', !data.errors?.length);

  const warnList = document.getElementById('resultWarnings');
  warnList.innerHTML = '';
  (data.warnings || []).forEach(w => {
    const li = document.createElement('li');
    li.textContent = w;
    warnList.appendChild(li);
  });
  document.getElementById('warningsSection').classList.toggle('hidden', !data.warnings?.length);
}

function renderError() {
  const box = document.getElementById('resultBox');
  box.classList.remove('hidden', 'result-valid');
  box.classList.add('result-invalid');
  document.getElementById('resultBadge').textContent = 'Error';
  document.getElementById('resultScore').textContent = '';
  document.getElementById('errorsSection').classList.add('hidden');
  document.getElementById('warningsSection').classList.add('hidden');
}

// ── Worker Auth ───────────────────────────────────────────────────────────────
function renderWorkerArea() {
  const area = document.getElementById('workerArea');
  if (state.token) {
    area.innerHTML = `
      <div class="worker-logged">
        <div class="worker-avatar">${(state.employeeName || 'U')[0].toUpperCase()}</div>
        <h2>${t('workerWelcome')}, ${state.employeeName ?? ''}!</h2>
        <button class="btn btn-secondary" onclick="logout()">${t('logoutBtn')}</button>
      </div>`;
  } else {
    area.innerHTML = `
      <div class="login-card">
        <h2 data-i18n="loginTitle">${t('loginTitle')}</h2>
        <form id="loginForm">
          <div class="form-group">
            <label>${t('labelEmployeeCode')}</label>
            <input type="text" id="empCode" required autocomplete="username" />
          </div>
          <div class="form-group">
            <label>${t('labelPassword')}</label>
            <input type="password" id="empPass" required autocomplete="current-password" />
          </div>
          <p id="loginErr" class="error-msg hidden">${t('loginError')}</p>
          <button type="submit" class="btn btn-primary">${t('btnLogin')}</button>
        </form>
      </div>`;

    document.getElementById('loginForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      const code = document.getElementById('empCode').value.trim();
      const pass = document.getElementById('empPass').value;
      const errEl = document.getElementById('loginErr');
      errEl.classList.add('hidden');

      try {
        const res = await fetch('/api/auth/login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ employeeCode: code, password: pass })
        });

        if (!res.ok) { errEl.classList.remove('hidden'); return; }

        const data = await res.json();
        const payload = JSON.parse(atob(data.token.split('.')[1]));
        state.token = data.token;
        state.employeeName = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ?? code;
        localStorage.setItem('token', state.token);
        localStorage.setItem('employeeName', state.employeeName);
        renderWorkerArea();
        updateNavAuth();
      } catch {
        errEl.classList.remove('hidden');
      }
    });
  }
}

function logout() {
  state.token = null;
  state.employeeName = null;
  localStorage.removeItem('token');
  localStorage.removeItem('employeeName');
  renderWorkerArea();
  updateNavAuth();
}

function updateNavAuth() {
  const badge = document.getElementById('authBadge');
  if (state.token && badge) {
    badge.textContent = state.employeeName?.[0]?.toUpperCase() ?? '?';
    badge.classList.remove('hidden');
  } else if (badge) {
    badge.classList.add('hidden');
  }
}

// ── Documents list ────────────────────────────────────────────────────────────
let docPage = 1;
async function loadDocuments(reset = true) {
  if (reset) docPage = 1;
  const tbody = document.getElementById('docTableBody');
  if (reset) tbody.innerHTML = '';

  try {
    const headers = {};
    if (state.token) headers['Authorization'] = 'Bearer ' + state.token;
    const res = await fetch(`/api/documents?page=${docPage}&pageSize=20`, { headers });

    if (!res.ok) {
      tbody.innerHTML = `<tr><td colspan="6" class="no-records">${t('noRecords')}</td></tr>`;
      return;
    }

    const data = await res.json();
    if (!data.items.length && reset) {
      tbody.innerHTML = `<tr><td colspan="6" class="no-records">${t('noRecords')}</td></tr>`;
    }

    for (const doc of data.items) {
      const tr = document.createElement('tr');
      const cells = [
        doc.documentType,
        doc.documentValue,
        null, // badge cell handled below
        `${doc.confidenceScore}%`,
        doc.country || '-',
        new Date(doc.validatedAt).toLocaleString()
      ];
      cells.forEach((text, i) => {
        const td = document.createElement('td');
        if (i === 2) {
          const span = document.createElement('span');
          span.className = `badge ${doc.isValid ? 'badge-valid' : 'badge-invalid'}`;
          span.textContent = doc.isValid ? '✓' : '✗';
          if (i === 1) td.className = 'doc-value';
          td.appendChild(span);
        } else {
          if (i === 1) td.className = 'doc-value';
          td.textContent = text;
        }
        tr.appendChild(td);
      });
      tbody.appendChild(tr);
    }

    document.getElementById('loadMoreDocs').classList.toggle(
      'hidden', data.items.length < 20 || docPage * 20 >= data.total);
  } catch {
    tbody.innerHTML = `<tr><td colspan="6" class="no-records">${t('noRecords')}</td></tr>`;
  }
}

document.getElementById('loadMoreDocs').addEventListener('click', () => {
  docPage++;
  loadDocuments(false);
});

// ── Clients list ──────────────────────────────────────────────────────────────
let clientPage = 1;
async function loadClients(reset = true) {
  if (reset) clientPage = 1;
  const tbody = document.getElementById('clientTableBody');
  if (reset) tbody.innerHTML = '';

  try {
    const headers = {};
    if (state.token) headers['Authorization'] = 'Bearer ' + state.token;
    const res = await fetch(`/api/clients?page=${clientPage}&pageSize=20`, { headers });

    if (!res.ok) {
      tbody.innerHTML = `<tr><td colspan="5" class="no-records">${t('noRecords')}</td></tr>`;
      return;
    }

    const data = await res.json();
    if (!data.items.length && reset) {
      tbody.innerHTML = `<tr><td colspan="5" class="no-records">${t('noRecords')}</td></tr>`;
    }

    for (const c of data.items) {
      const tr = document.createElement('tr');
      [c.name, c.email, c.documentType, c.country || '-', new Date(c.createdAt).toLocaleString()].forEach(text => {
        const td = document.createElement('td');
        td.textContent = text;
        tr.appendChild(td);
      });
      tbody.appendChild(tr);
    }

    document.getElementById('loadMoreClients').classList.toggle(
      'hidden', data.items.length < 20 || clientPage * 20 >= data.total);
  } catch {
    tbody.innerHTML = `<tr><td colspan="5" class="no-records">${t('noRecords')}</td></tr>`;
  }
}

document.getElementById('loadMoreClients').addEventListener('click', () => {
  clientPage++;
  loadClients(false);
});

// ── Init ──────────────────────────────────────────────────────────────────────
document.getElementById('langSelect').value = state.lang;
document.getElementById('langSelect').addEventListener('change', e => setLang(e.target.value));
document.getElementById('countrySelect').addEventListener('change', buildDocTypeSelect);
document.getElementById('docTypeSelect').addEventListener('change', updateDocInput);
document.getElementById('docValue').addEventListener('input', onDocValueInput);

applyTranslations();
showPage('home');
updateNavAuth();
