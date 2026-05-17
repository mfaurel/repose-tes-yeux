const CIRCUMFERENCE = 2 * Math.PI * 52; // matches r=52 in SVG

async function init() {
  const durationMs = await window.overlay.getRestDuration();
  const durationSec = durationMs / 1000;

  const progressCircle = document.getElementById('progressCircle');
  const countdownEl    = document.getElementById('countdown');
  const dismissBtn     = document.getElementById('dismissBtn');

  progressCircle.style.strokeDasharray  = CIRCUMFERENCE;
  progressCircle.style.strokeDashoffset = 0;

  let remaining = durationSec;

  const tick = () => {
    remaining -= 1;
    if (remaining < 0) remaining = 0;

    countdownEl.textContent = remaining;

    const fraction = remaining / durationSec;
    progressCircle.style.strokeDashoffset = CIRCUMFERENCE * (1 - fraction);

    if (remaining <= 0) {
      clearInterval(timer);
    }
  };

  const timer = setInterval(tick, 1000);
  tick(); // immediate first tick to sync display

  dismissBtn.addEventListener('click', () => {
    clearInterval(timer);
    window.overlay.dismiss();
  });
}

init();
