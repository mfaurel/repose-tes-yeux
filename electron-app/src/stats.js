let count = 0;
let date = new Date().toDateString();

function increment() {
  const today = new Date().toDateString();
  if (today !== date) { count = 0; date = today; }
  count++;
}

function getToday() {
  const today = new Date().toDateString();
  if (today !== date) { count = 0; date = today; }
  return count;
}

module.exports = { increment, getToday };
