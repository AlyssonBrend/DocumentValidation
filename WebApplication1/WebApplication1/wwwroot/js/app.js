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
  const type = document.getElementById('docTypeSelect').value;
  const value = document.getElementById('docValue').value.trim();

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
      tr.innerHTML = `
        <td>${doc.documentType}</td>
        <td class="doc-value">${doc.documentValue}</td>
        <td><span class="badge ${doc.isValid ? 'badge-valid' : 'badge-invalid'}">${doc.isValid ? '✓' : '✗'}</span></td>
        <td>${doc.confidenceScore}%</td>
        <td>${doc.country || '-'}</td>
        <td>${new Date(doc.validatedAt).toLocaleString()}</td>`;
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
      tr.innerHTML = `
        <td>${c.name}</td>
        <td>${c.email}</td>
        <td>${c.documentType}</td>
        <td>${c.country || '-'}</td>
        <td>${new Date(c.createdAt).toLocaleString()}</td>`;
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

applyTranslations();
showPage('home');
updateNavAuth();
