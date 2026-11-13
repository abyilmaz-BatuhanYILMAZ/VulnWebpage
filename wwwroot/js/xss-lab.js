// Unsafe and safe DOM sinks are kept side-by-side for the lab.
(() => {
  const runButton = document.getElementById('runDomDemo');
  const inputBox = document.getElementById('domInputBox');
  const unsafeTarget = document.getElementById('domUnsafeTarget');
  const safeTarget = document.getElementById('domSafeTarget');
  if (!runButton || !inputBox || !unsafeTarget || !safeTarget) return;

  function runDomDemo() {
    const value = inputBox.value || '';
    unsafeTarget.innerHTML = value;
    safeTarget.textContent = value;
  }

  runButton.addEventListener('click', runDomDemo);
  runDomDemo();
})();
