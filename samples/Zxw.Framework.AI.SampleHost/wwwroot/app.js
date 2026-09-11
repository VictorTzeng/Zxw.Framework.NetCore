// OrcaRouter provider configuration UI.
//
// Two authentication choices feed one credential seam; the model dropdown is rebuilt from the
// live catalog for whatever capability the current input state requires. The API key never
// reaches this file: the server holds it and returns model metadata only.
(function () {
  'use strict';

  var state = {
    // Monotonically increasing sign-in generation. Every async response and poll iteration must
    // confirm it still belongs to the current generation before it may change credentials or UI.
    generation: 0,
    attemptId: null,
    busy: false,
    models: [],
    selectedModelId: null,
    connection: null
  };

  var els = {
    apiKeyInput: document.getElementById('apiKeyInput'),
    saveKeyBtn: document.getElementById('saveKeyBtn'),
    clearKeyBtn: document.getElementById('clearKeyBtn'),
    apiKeyStatus: document.getElementById('apiKeyStatus'),
    connectBtn: document.getElementById('connectBtn'),
    cancelConnectBtn: document.getElementById('cancelConnectBtn'),
    connectBox: document.getElementById('connectBox'),
    connectHint: document.getElementById('connectHint'),
    authorizeUrl: document.getElementById('authorizeUrl'),
    codeInput: document.getElementById('codeInput'),
    completeConnectBtn: document.getElementById('completeConnectBtn'),
    pkceStatus: document.getElementById('pkceStatus'),
    imageAttachment: document.getElementById('imageAttachment'),
    modelSelector: document.getElementById('modelSelector'),
    modelTrigger: document.getElementById('modelTrigger'),
    modelTriggerLabel: document.getElementById('modelTriggerLabel'),
    modelListbox: document.getElementById('modelListbox'),
    modelStatus: document.getElementById('modelStatus'),
    selectionNotice: document.getElementById('selectionNotice')
  };

  // ------------------------------------------------------------------ helpers

  function setStatus(el, text, kind) {
    el.textContent = text || '';
    el.className = 'status' + (kind ? ' ' + kind : '');
  }

  function setBusy(busy, hint) {
    state.busy = busy;
    els.connectBtn.disabled = busy;
    els.completeConnectBtn.disabled = busy;
    els.cancelConnectBtn.disabled = !busy;
    if (busy) {
      els.connectBox.hidden = false;
      if (hint) setStatus(els.connectHint, hint);
    }
  }

  /** Clear the sign-in UI. Used by every terminal path, including pagehide. */
  function clearConnectUi(message, kind) {
    state.attemptId = null;
    setBusy(false);
    els.connectBox.hidden = true;
    els.authorizeUrl.textContent = '';
    els.codeInput.value = '';
    setStatus(els.connectHint, '');
    if (message) setStatus(els.pkceStatus, message, kind);
  }

  function postJson(url, body) {
    return fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body || {})
    }).then(function (r) { return r.json(); });
  }

  // ------------------------------------------------------------------ credentials

  function loadStatus() {
    return fetch('/api/orcarouter/status').then(function (r) { return r.json(); }).then(function (data) {
      state.connection = data;
      var api = data.apiKey;
      setStatus(els.apiKeyStatus,
        api.message + (api.maskedKey ? ' (' + api.maskedKey + ')' : ''),
        api.state === 'Active' ? 'active' : (api.state === 'NeedsReauth' ? 'error' : ''));
      els.clearKeyBtn.disabled = !api.canClear;

      var pkce = data.pkce;
      if (pkce.state === 'Active') {
        setStatus(els.pkceStatus, pkce.message + (pkce.maskedKey ? ' (' + pkce.maskedKey + ')' : ''), 'active');
      }
      return data;
    });
  }

  els.saveKeyBtn.addEventListener('click', function () {
    postJson('/api/orcarouter/apikey', { apiKey: els.apiKeyInput.value }).then(function (data) {
      var s = data.apiKey;
      els.apiKeyInput.value = '';
      setStatus(els.apiKeyStatus, s.message, s.state === 'Active' ? 'active' : 'error');
      els.clearKeyBtn.disabled = !s.canClear;
      return refreshModels();
    });
  });

  els.clearKeyBtn.addEventListener('click', function () {
    fetch('/api/orcarouter/apikey', { method: 'DELETE' })
      .then(function (r) { return r.json(); })
      .then(function (data) {
        els.apiKeyInput.value = '';
        setStatus(els.apiKeyStatus, data.apiKey.message, '');
        els.clearKeyBtn.disabled = true;
        return refreshModels();
      });
  });

  // ------------------------------------------------------------------ connect (PKCE)

  els.connectBtn.addEventListener('click', function () {
    // A new sign-in always takes a new generation, so any in-flight response from a previous
    // attempt is invalidated rather than allowed to overwrite this one.
    var generation = ++state.generation;
    setStatus(els.pkceStatus, '');
    setBusy(true, 'Requesting an authorization URL…');

    postJson('/api/orcarouter/connect', {}).then(function (data) {
      if (generation !== state.generation) return;
      state.attemptId = data.attemptId;
      els.authorizeUrl.textContent = data.authorizeUrl;
      setStatus(els.connectHint, data.instructions);
    }).catch(function () {
      if (generation !== state.generation) return;
      clearConnectUi('Could not start the sign-in. Check the network and try again.', 'error');
    });
  });

  els.completeConnectBtn.addEventListener('click', function () {
    var generation = state.generation;
    setBusy(true, 'Exchanging the code…');
    postJson('/api/orcarouter/connect/complete', {
      attemptId: state.attemptId,
      code: els.codeInput.value
    }).then(function (data) {
      if (generation !== state.generation) return;
      if (data.outcome === 'connected') {
        clearConnectUi(data.message, 'active');
        loadStatus();
        refreshModels();
      } else if (data.outcome === 'stale') {
        clearConnectUi(data.message, 'warn');
      } else {
        // Denial, exchange error, rate limit and network failures all land here and all release
        // the lock, so the user can immediately try again.
        clearConnectUi(data.message, 'error');
      }
    }).catch(function () {
      if (generation !== state.generation) return;
      clearConnectUi('The sign-in could not be completed. Try again.', 'error');
    });
  });

  els.cancelConnectBtn.addEventListener('click', function () {
    cancelConnect('Sign-in cancelled.');
  });

  function cancelConnect(message) {
    var attemptId = state.attemptId;
    // Bump first: a response already in flight must not repopulate the cleared UI.
    state.generation++;
    clearConnectUi(message, 'warn');
    if (attemptId) {
      postJson('/api/orcarouter/connect/cancel', { attemptId: attemptId }).catch(function () { /* released server-side by timeout */ });
    }
  }

  // window close, reload, and back-forward-cache entry. This handler must clear busy and hint
  // synchronously rather than relying on the guarded cleanup of the invalidated request, which
  // would correctly refuse to mutate state and leave a restored page permanently busy.
  window.addEventListener('pagehide', function () {
    var attemptId = state.attemptId;
    state.generation++;
    state.attemptId = null;
    state.busy = false;

    // Synchronous UI reset. No async callback is needed and none is trusted to run.
    els.connectBox.hidden = true;
    els.connectBtn.disabled = false;
    els.completeConnectBtn.disabled = false;
    els.cancelConnectBtn.disabled = true;
    els.connectHint.textContent = '';
    els.authorizeUrl.textContent = '';
    els.codeInput.value = '';

    // Tell the server to release the sign-in lock, with keepalive so the request survives unload.
    if (attemptId) {
      var payload = JSON.stringify({ attemptId: attemptId });
      try {
        if (navigator.sendBeacon) {
          navigator.sendBeacon('/api/orcarouter/connect/cancel',
            new Blob([payload], { type: 'application/json' }));
        } else {
          fetch('/api/orcarouter/connect/cancel', {
            method: 'POST', headers: { 'Content-Type': 'application/json' },
            body: payload, keepalive: true
          });
        }
      } catch (e) { /* the server also releases on timeout */ }
    }
  });

  // ------------------------------------------------------------------ model selector

  function currentCapability() {
    return els.imageAttachment.checked ? 'multimodal' : 'chat';
  }

  function currentModalities() {
    return els.imageAttachment.checked ? 'image' : '';
  }

  function refreshModels() {
    var capability = currentCapability();
    var modalities = currentModalities();

    setStatus(els.modelStatus, 'Loading models…');
    return fetch('/api/orcarouter/models?capability=' + encodeURIComponent(capability) +
                 '&modalities=' + encodeURIComponent(modalities))
      .then(function (r) { return r.json(); })
      .then(function (data) {
        state.models = data.models || [];
        renderOptions();

        var badge = data.authoritative
          ? '<span class="badge live">live</span>'
          : '<span class="badge">verified fallback</span>';
        setStatus(els.modelStatus,
          data.capability + ': ' + state.models.length + ' models ' +
          (data.message ? '— ' + data.message : ''),
          data.degraded ? 'warn' : '');

        // A selection that the current capability no longer admits must be cleared, not kept.
        if (state.selectedModelId &&
            !state.models.some(function (m) { return m.id === state.selectedModelId; })) {
          var stale = state.selectedModelId;
          state.selectedModelId = null;
          els.modelTriggerLabel.textContent = 'Select a model';
          setStatus(els.selectionNotice,
            '"' + stale + '" is not available for ' + capability +
            (modalities ? ' with an image attached' : '') +
            ' and was cleared. Choose one of the listed models.', 'warn');
        } else {
          setStatus(els.selectionNotice, '');
        }
      })
      .catch(function () {
        state.models = [];
        renderOptions();
        setStatus(els.modelStatus, 'Could not load the model list.', 'error');
      });
  }

  function renderOptions() {
    els.modelListbox.innerHTML = '';
    if (!state.models.length) {
      var empty = document.createElement('div');
      empty.className = 'empty';
      empty.textContent = 'No model in the catalog supports this input.';
      els.modelListbox.appendChild(empty);
      return;
    }

    state.models.forEach(function (model) {
      var option = document.createElement('div');
      option.className = 'option';
      option.setAttribute('role', 'option');
      option.setAttribute('data-model-id', model.id);
      option.setAttribute('aria-selected',
        String(model.id === state.selectedModelId));

      var id = document.createElement('div');
      id.className = 'opt-id';
      id.textContent = model.id;

      var meta = document.createElement('div');
      meta.className = 'opt-meta';
      var bits = [];
      if (model.contextLength) bits.push((model.contextLength / 1000) + 'k ctx');
      bits.push('(' + ((model.inputModalities || ['not declared']).join(', ')) + ')');
      meta.textContent = bits.join(' · ');

      option.appendChild(id);
      option.appendChild(meta);
      option.addEventListener('click', function () { selectModel(model.id); });
      els.modelListbox.appendChild(option);
    });
  }

  function selectModel(id) {
    state.selectedModelId = id;
    els.modelTriggerLabel.textContent = id;
    setStatus(els.selectionNotice, '');
    closePanel();
  }

  function openPanel() {
    els.modelListbox.hidden = false;
    els.modelTrigger.setAttribute('aria-expanded', 'true');
  }

  function closePanel() {
    els.modelListbox.hidden = true;
    els.modelTrigger.setAttribute('aria-expanded', 'false');
  }

  els.modelTrigger.addEventListener('click', function () {
    if (els.modelListbox.hidden) openPanel(); else closePanel();
  });

  document.addEventListener('click', function (event) {
    if (!els.modelSelector.contains(event.target)) closePanel();
  });

  els.imageAttachment.addEventListener('change', function () {
    // The attachment changes which models are compatible, so the options are recomputed and an
    // incompatible selection is cleared rather than silently kept.
    refreshModels();
  });

  // Exposed so the automated UI test can assert generation/busy bookkeeping directly.
  window.__orcaUiState = state;

  loadStatus().then(refreshModels);
})();
