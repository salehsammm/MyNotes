const getElement = id => document.getElementById(id);
const svgNS = 'http://www.w3.org/2000/svg';
let selectedDays = 30;
let toastTimer;
let currentData = [];
let loadSequence = 0;

function notify(message, error = false) {
  const toast = getElement('toast');
  toast.textContent = message;
  toast.className = `toast show${error ? ' error' : ''}`;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => toast.className = 'toast', 3500);
}

function svg(tag, attributes = {}, text = null) {
  const node = document.createElementNS(svgNS, tag);
  for (const [key, value] of Object.entries(attributes)) node.setAttribute(key, value);
  if (text !== null) node.textContent = text;
  return node;
}

function renderChart(data) {
  const chart = getElement('historyChart');
  chart.replaceChildren();
  const width = Math.max(280, chart.clientWidth), height = Math.max(240, chart.clientHeight);
  chart.setAttribute('viewBox', `0 0 ${width} ${height}`);
  const left = 40, right = 15, top = 20, bottom = 38;
  const plotWidth = width - left - right, plotHeight = height - top - bottom;
  const counts = data.map(point => point.count);
  let min = Math.max(0, Math.min(...counts) - 2);
  let max = Math.max(...counts) + 2;
  if (max <= min) max = min + 1;
  const x = index => left + (data.length === 1 ? plotWidth / 2 : index / (data.length - 1) * plotWidth);
  const y = count => top + (max - count) / (max - min) * plotHeight;

  for (let index = 0; index <= 4; index++) {
    const value = min + (max - min) * (4 - index) / 4;
    const yy = top + plotHeight * index / 4;
    chart.append(svg('line', { x1: left, y1: yy, x2: width - right, y2: yy, stroke: '#35403e', 'stroke-dasharray': '3 6' }));
    chart.append(svg('text', { x: 0, y: yy + 4, fill: '#86948f', 'font-size': 11 }, String(Math.round(value))));
  }

  const line = data.map((point, index) => `${index ? 'L' : 'M'} ${x(index).toFixed(1)} ${y(point.count).toFixed(1)}`).join(' ');
  const area = `${line} L ${x(data.length - 1).toFixed(1)} ${top + plotHeight} L ${x(0).toFixed(1)} ${top + plotHeight} Z`;
  const defs = svg('defs');
  const gradient = svg('linearGradient', { id: 'areaGradient', x1: '0', y1: '0', x2: '0', y2: '1' });
  gradient.append(svg('stop', { offset: '0%', 'stop-color': '#b8ef76', 'stop-opacity': '.22' }), svg('stop', { offset: '100%', 'stop-color': '#b8ef76', 'stop-opacity': '0' }));
  defs.append(gradient);
  chart.append(defs, svg('path', { d: area, fill: 'url(#areaGradient)' }), svg('path', { d: line, fill: 'none', stroke: '#b8ef76', 'stroke-width': 3, 'stroke-linecap': 'round', 'stroke-linejoin': 'round', 'vector-effect': 'non-scaling-stroke' }));

  const labelIndices = [...new Set([0, Math.floor((data.length - 1) / 2), data.length - 1])];
  labelIndices.forEach(index => {
    const date = new Date(`${data[index].day}T00:00:00`);
    chart.append(svg('text', { x: x(index), y: height - 5, fill: '#86948f', 'font-size': 11, 'text-anchor': index === 0 ? 'start' : index === data.length - 1 ? 'end' : 'middle' }, date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })));
  });
  data.forEach((point, index) => {
    const circle = svg('circle', { cx: x(index), cy: y(point.count), r: data.length <= 30 ? 4 : 2.5, fill: '#b8ef76', stroke: '#19231b', 'stroke-width': 2, 'vector-effect': 'non-scaling-stroke' });
    circle.append(svg('title', {}, `${point.day}: ${point.count} bookmarks`));
    chart.append(circle);
  });
}

async function loadHistory() {
  const sequence = ++loadSequence;
  const message = getElement('chartMessage');
  message.hidden = false;
  message.textContent = 'Loading history…';
  try {
    const response = await fetch(`/bookmarks/api/history?days=${selectedDays}`);
    if (!response.ok) throw new Error('Could not fetch history');
    const data = await response.json();
    if (sequence !== loadSequence) return;
    if (!data.length) {
      currentData = [];
      getElement('historyChart').replaceChildren();
      getElement('currentValue').textContent = '—';
      getElement('changeValue').textContent = 'No history in this range yet';
      getElement('changeValue').className = 'history-change';
      message.textContent = 'No counts recorded yet. Use “Log count now” to start.';
      return;
    }
    const first = data[0].count, last = data[data.length - 1].count, difference = last - first;
    getElement('currentValue').textContent = last.toLocaleString();
    getElement('changeValue').textContent = difference === 0 ? 'No change in this range' : `${difference > 0 ? '+' : '−'}${Math.abs(difference)} bookmarks in this range`;
    getElement('changeValue').className = `history-change${difference < 0 ? ' good' : difference > 0 ? ' bad' : ''}`;
    currentData = data;
    renderChart(data);
    message.hidden = true;
  } catch {
    if (sequence !== loadSequence) return;
    currentData = [];
    getElement('historyChart').replaceChildren();
    message.textContent = 'Could not load history. Please try again.';
    notify('Could not load bookmark history.', true);
  }
}

document.querySelectorAll('[data-days]').forEach(button => button.addEventListener('click', () => {
  selectedDays = Number(button.dataset.days);
  document.querySelectorAll('[data-days]').forEach(other => {
    const selected = other === button;
    other.classList.toggle('selected', selected);
    other.setAttribute('aria-pressed', String(selected));
  });
  loadHistory();
}));

getElement('logButton').addEventListener('click', async () => {
  const button = getElement('logButton');
  button.disabled = true;
  try {
    const response = await fetch('/bookmarks/api/log', { method: 'POST' });
    if (!response.ok) throw new Error('Could not log count');
    await loadHistory();
    notify('Current count saved.');
  } catch {
    notify('Could not save the current count.', true);
  } finally {
    button.disabled = false;
  }
});

loadHistory();
new ResizeObserver(() => { if (currentData.length) renderChart(currentData); }).observe(getElement('historyChart'));
