(() => {
  'use strict';
  const root = document.querySelector('[data-progress-bar]');
  if (!root) return;

  const key = 'vulnwebpage.practice.v1';
  const inputs = Array.from(document.querySelectorAll('[data-progress-check]'));
  const validIds = new Set(inputs.map(input => input.dataset.progressCheck));
  const count = document.querySelector('[data-progress-label]');
  const fill = document.querySelector('[data-progress-fill]');
  const reset = document.querySelector('[data-progress-reset]');

  let finished = new Set();
  try {
    const value = JSON.parse(localStorage.getItem(key) || '[]');
    if (Array.isArray(value)) {
      finished = new Set(value.filter(item => typeof item === 'string' && validIds.has(item)));
    }
  } catch {
    finished = new Set();
  }

  function render() {
    for (const input of inputs) {
      input.checked = finished.has(input.dataset.progressCheck);
    }
    const completed = finished.size;
    const total = inputs.length;
    const percentage = total ? (completed / total) * 100 : 0;
    root.setAttribute('aria-valuenow', String(completed));
    fill.style.width = percentage + '%';
    count.textContent = completed + ' / ' + total + ' modules marked complete';
  }

  function persist() {
    try {
      localStorage.setItem(key, JSON.stringify([...finished]));
    } catch {
      // Browser storage can be disabled; checkboxes still work this session.
    }
    render();
  }

  for (const input of inputs) {
    input.addEventListener('change', () => {
      const id = input.dataset.progressCheck;
      if (input.checked) finished.add(id);
      else finished.delete(id);
      persist();
    });
  }

  reset.addEventListener('click', () => {
    finished.clear();
    persist();
  });

  render();
})();
