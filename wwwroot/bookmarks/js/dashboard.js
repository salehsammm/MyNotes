const getElement = id => document.getElementById(id);
const selectedDate = new Date();
selectedDate.setHours(0, 0, 0, 0);
let loadSequence = 0;
let toastTimer;

function today() {
  const date = new Date();
  date.setHours(0, 0, 0, 0);
  return date;
}

function dateKey(date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
}

function notify(message, error = false) {
  const toast = getElement('toast');
  toast.textContent = message;
  toast.className = `toast show${error ? ' error' : ''}`;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => toast.className = 'toast', 3500);
}

async function request(url, options) {
  const response = await fetch(url, options);
  if (!response.ok) throw new Error(`Request failed (${response.status})`);
  const body = await response.text();
  return body ? JSON.parse(body) : null;
}

async function refreshCount() {
  try {
    const data = await request('/bookmarks/api/count');
    getElement('bookmarkCount').textContent = Number(data.count).toLocaleString();
    getElement('folderName').textContent = data.folder || 'Bookmark folder';
    const date = new Date(data.at);
    getElement('countStatus').textContent = `Updated ${date.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })}`;
    getElement('countStatus').classList.remove('error');
  } catch {
    getElement('countStatus').textContent = 'Unable to refresh';
    getElement('countStatus').classList.add('error');
  }
}

function updateDateNavigation() {
  const isToday = dateKey(selectedDate) === dateKey(today());
  getElement('dateLabel').textContent = selectedDate.toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric' });
  getElement('jumpDate').value = dateKey(selectedDate);
  getElement('todayButton').hidden = isToday;
  getElement('viewingHint').textContent = isToday ? 'Viewing today' : dateKey(selectedDate) < dateKey(today()) ? 'Viewing an earlier day' : 'Viewing a future day';
  updateResets();
}

function countdown(target) {
  const minutes = Math.max(0, Math.floor((target - new Date()) / 60000));
  const days = Math.floor(minutes / 1440);
  const hours = Math.floor((minutes % 1440) / 60);
  const mins = minutes % 60;
  if (days) return `${days}d ${hours}h`;
  if (hours) return `${hours}h ${mins}m`;
  return `${mins}m`;
}

function updateResets() {
  if (dateKey(selectedDate) !== dateKey(today())) {
    getElement('dailyReset').textContent = 'Progress for this day';
    getElement('weeklyReset').textContent = 'Progress for this week';
    return;
  }
  const midnight = new Date();
  midnight.setHours(24, 0, 0, 0);
  const saturday = today();
  let days = (6 - saturday.getDay() + 7) % 7;
  if (!days) days = 7;
  saturday.setDate(saturday.getDate() + days);
  getElement('dailyReset').textContent = `Resets in ${countdown(midnight)}`;
  getElement('weeklyReset').textContent = `Resets in ${countdown(saturday)}`;
}

async function loadChecklist() {
  const sequence = ++loadSequence;
  updateDateNavigation();
  try {
    const items = await request(`/bookmarks/api/checklist?date=${dateKey(selectedDate)}`);
    if (sequence !== loadSequence) return;
    renderList('daily', items.filter(item => item.recurrence === 'Daily'));
    renderList('weekly', items.filter(item => item.recurrence === 'Weekly'));
    await loadMissed();
  } catch {
    if (sequence !== loadSequence) return;
    for (const kind of ['daily', 'weekly']) {
      const list = getElement(`${kind}Items`);
      list.replaceChildren(emptyState('Could not load items', 'Try refreshing the page.'));
    }
    notify('Could not load the checklist.', true);
  }
}

async function loadMissed() {
  const list = getElement('missedItems');
  try {
    const items = await request('/bookmarks/api/checklist/missed');
    getElement('missedSummary').textContent = `${items.length} open`;
    list.replaceChildren();
    if (!items.length) {
      list.append(emptyState('All caught up', 'No unfinished tasks from past periods.'));
      return;
    }
    items.forEach(item => list.append(renderItem(item, true)));
  } catch {
    list.replaceChildren(emptyState('Could not load missed tasks', 'Try refreshing the page.'));
  }
}

async function loadQuestionPreview() {
  const list = getElement('questionPreviewList');
  try {
    const questions = await request('/bookmarks/api/questions');
    const open = questions.filter(q => !q.isAnswered);
    const answered = questions.filter(q => q.isAnswered);
    getElement('questionCounts').textContent = `${open.length} unanswered · ${answered.length} answered`;
    list.replaceChildren();
    if (!questions.length) {
      list.append(emptyState('No questions yet', 'Keep a curiosity here whenever it comes to mind.'));
      return;
    }
    for (const q of [...open.slice(0, 3), ...answered.slice(0, 1)]) {
      const link = document.createElement('a');
      link.href = '/bookmarks/questions';
      link.className = 'question-preview-item';
      const text = document.createElement('span');
      text.textContent = q.text;
      const status = document.createElement('span');
      status.className = `question-status${q.isAnswered ? ' answered' : ''}`;
      status.textContent = q.isAnswered ? 'Answered' : 'Open';
      link.append(text, status);
      list.append(link);
    }
  } catch {
    getElement('questionCounts').textContent = 'Questions unavailable';
    list.replaceChildren();
  }
}

function emptyState(title, detail) {
  const div = document.createElement('div');
  div.className = 'empty-state';
  const strong = document.createElement('strong');
  strong.textContent = title;
  const span = document.createElement('span');
  span.textContent = detail;
  div.append(strong, span);
  return div;
}

function renderList(kind, items) {
  const list = getElement(`${kind}Items`);
  list.replaceChildren();
  const required = items.filter(item => !item.isOptional);
  const completed = required.filter(item => item.completed).length;
  getElement(`${kind}Summary`).textContent = required.length ? `${completed} of ${required.length} done` : 'No required tasks';
  getElement(`${kind}Progress`).style.width = `${required.length ? completed / required.length * 100 : 0}%`;
  if (!items.length) {
    list.append(emptyState('A fresh start', 'Add an item to begin tracking.'));
    return;
  }
  items.filter(item => !item.completed && !item.isOptional)
    .concat(items.filter(item => !item.completed && item.isOptional), items.filter(item => item.completed))
    .forEach(item => list.append(renderItem(item)));
}

function renderItem(item, missed = false) {
  const row = document.createElement('div');
  row.className = `item${item.completed ? ' complete' : ''}${item.isOptional ? ' optional' : ''}`;
  row.innerHTML = '<span class="completion-mark" aria-hidden="true">✓</span><div class="item-main"><div class="item-title-line"><div class="item-title"></div><span class="optional-badge" hidden>Optional</span></div><div class="item-subtitle"></div></div><div class="item-actions"><div class="stepper"><button class="minus" type="button">−</button><span></span><button class="plus" type="button">+</button></div><button class="more-button" type="button" aria-label="Item options" aria-expanded="false">⋯</button></div>';
  row.querySelector('.item-title').textContent = item.title;
  row.querySelector('.optional-badge').hidden = !item.isOptional;
  const period = missed ? `${item.recurrence === 'Daily' ? 'Day' : 'Week of'} ${item.periodStart} · ` : '';
  const cadence = !item.isOneTime && item.recurrence === 'Daily' && item.repeatEveryDays > 1
    ? `Every ${item.repeatEveryDays} days · ` : '';
  row.querySelector('.item-subtitle').textContent = `${period}${item.isOneTime ? 'One-time · ' : cadence}${item.completed ? 'Completed' : `${Math.max(0, item.targetCount - item.currentCount)} to go`}`;
  row.querySelector('.stepper span').textContent = `${item.currentCount}/${item.targetCount}`;
  const minus = row.querySelector('.minus');
  const plus = row.querySelector('.plus');
  minus.setAttribute('aria-label', `Decrease ${item.title}`);
  plus.setAttribute('aria-label', `Increase ${item.title}`);
  minus.disabled = !item.progressId || item.currentCount <= 0;
  plus.disabled = !item.progressId || item.currentCount >= item.targetCount;
  minus.addEventListener('click', () => changeProgress(item, 'decrement', row));
  plus.addEventListener('click', () => changeProgress(item, 'increment', row));
  row.querySelector('.more-button').addEventListener('click', event => {
    event.stopPropagation();
    document.querySelectorAll('.item-menu').forEach(menu => menu.remove());
    document.querySelectorAll('.more-button').forEach(button => button.setAttribute('aria-expanded', 'false'));
    const menu = document.createElement('div');
    menu.className = 'item-menu';
    menu.innerHTML = '<button type="button" class="edit">Edit item</button><button type="button" class="delete">Delete item</button>';
    menu.querySelector('.edit').addEventListener('click', () => { menu.remove(); showForm(item.recurrence, item, row); });
    menu.querySelector('.delete').addEventListener('click', () => { menu.remove(); deleteItem(item); });
    row.append(menu);
    row.querySelector('.more-button').setAttribute('aria-expanded', 'true');
  });
  return row;
}

async function changeProgress(item, action, row) {
  row.querySelectorAll('button').forEach(button => button.disabled = true);
  try {
    await request(`/bookmarks/api/checklist/progress/${item.progressId}/${action}`, { method: 'POST' });
    await loadChecklist();
  } catch {
    notify('Could not update progress.', true);
    await loadChecklist();
  }
}

function showForm(recurrence, item = null, row = null) {
  const kind = recurrence.toLowerCase();
  document.querySelectorAll('.item-form').forEach(form => form.remove());
  document.querySelectorAll('.item.hidden-for-edit').forEach(element => element.classList.remove('hidden-for-edit'));
  const form = document.createElement('form');
  form.className = 'item-form';
  form.innerHTML = '<div><label for="itemTitle">Item</label><input id="itemTitle" name="title" type="text" maxlength="200" placeholder="e.g. Watch one saved video" required></div><div><label for="itemTarget">Goal</label><input id="itemTarget" name="target" type="number" min="1" max="999" required></div><label class="one-time-option"><input name="oneTime" type="checkbox"> One-time task</label><label class="one-time-option"><input name="optional" type="checkbox"> Optional task <small>(never appears in Missed tasks)</small></label><div class="repeat-field"><label for="repeatEveryDays">Repeat every (days)</label><input id="repeatEveryDays" name="repeatEveryDays" type="number" min="1" max="365" required></div><div class="first-due-field"><label for="firstDueDate">First due date</label><input id="firstDueDate" name="firstDueDate" type="date" required></div><div class="schedule-field" hidden><label for="scheduledDate">Scheduled date</label><input id="scheduledDate" name="scheduledDate" type="date"></div><p class="form-error" hidden></p><div class="form-actions"><button type="button" class="cancel">Cancel</button><button type="submit">Save item</button></div>';
  const title = form.elements.title;
  const target = form.elements.target;
  title.value = item?.title || '';
  target.value = item?.targetCount || 1;
  const oneTime = form.elements.oneTime;
  const scheduledDate = form.elements.scheduledDate;
  const repeatEveryDays = form.elements.repeatEveryDays;
  const firstDueDate = form.elements.firstDueDate;
  oneTime.checked = item ? item.isOneTime : true;
  form.elements.optional.checked = item?.isOptional || false;
  scheduledDate.value = item?.periodStart || dateKey(selectedDate);
  repeatEveryDays.value = item?.repeatEveryDays || 1;
  firstDueDate.value = item?.firstDueDate || dateKey(selectedDate);
  const syncSchedule = () => {
    form.querySelector('.schedule-field').hidden = !oneTime.checked;
    scheduledDate.required = oneTime.checked;
    const repeat = recurrence === 'Daily' && !oneTime.checked;
    form.querySelector('.repeat-field').hidden = !repeat;
    form.querySelector('.first-due-field').hidden = !repeat;
    repeatEveryDays.required = repeat;
    firstDueDate.required = repeat;
  };
  oneTime.addEventListener('change', syncSchedule);
  syncSchedule();
  const close = () => { form.remove(); if (row) row.classList.remove('hidden-for-edit'); };
  form.querySelector('.cancel').addEventListener('click', close);
  form.addEventListener('submit', async event => {
    event.preventDefault();
    const trimmed = title.value.trim();
    const goal = Number(target.value);
    const interval = Number(repeatEveryDays.value);
    const error = form.querySelector('.form-error');
    if (!trimmed || !Number.isInteger(goal) || goal < 1 ||
        (recurrence === 'Daily' && !oneTime.checked && (!Number.isInteger(interval) || interval < 1 || interval > 365))) {
      error.textContent = 'Enter a name, a goal, and a repeat interval from 1 to 365 days.';
      error.hidden = false;
      return;
    }
    const submit = form.querySelector('[type=submit]');
    submit.disabled = true;
    try {
      await request(item ? `/bookmarks/api/checklist/${item.id}` : '/bookmarks/api/checklist', { method: item ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ title: trimmed, recurrence, targetCount: goal, isOneTime: oneTime.checked, isOptional: form.elements.optional.checked, scheduledDate: oneTime.checked ? scheduledDate.value : null, repeatEveryDays: recurrence === 'Daily' && !oneTime.checked ? interval : 1, firstDueDate: recurrence === 'Daily' && !oneTime.checked ? firstDueDate.value : null }) });
      close();
      await loadChecklist();
      notify(item ? 'Item updated.' : oneTime.checked ? 'One-time task scheduled.' : 'Item added.');
    } catch {
      error.textContent = 'Could not save this item. Please try again.';
      error.hidden = false;
      submit.disabled = false;
    }
  });
  if (row) {
    row.classList.add('hidden-for-edit');
    row.after(form);
  } else {
    getElement(`${kind}Items`).after(form);
  }
  title.focus();
}

async function deleteItem(item) {
  if (!confirm(`Delete “${item.title}”?`)) return;
  try {
    await request(`/bookmarks/api/checklist/${item.id}`, { method: 'DELETE' });
    await loadChecklist();
    notify('Item deleted.');
  } catch {
    notify('Could not delete this item.', true);
  }
}

document.addEventListener('click', event => {
  if (!event.target.closest('.more-button, .item-menu')) {
    document.querySelectorAll('.item-menu').forEach(menu => menu.remove());
    document.querySelectorAll('.more-button').forEach(button => button.setAttribute('aria-expanded', 'false'));
  }
});
document.querySelectorAll('[data-add]').forEach(button => button.addEventListener('click', () => showForm(button.dataset.add)));
getElement('previousDay').addEventListener('click', () => { selectedDate.setDate(selectedDate.getDate() - 1); loadChecklist(); });
getElement('nextDay').addEventListener('click', () => { selectedDate.setDate(selectedDate.getDate() + 1); loadChecklist(); });
getElement('jumpDate').addEventListener('change', event => { if (event.target.value) { selectedDate.setTime(new Date(`${event.target.value}T00:00:00`).getTime()); loadChecklist(); } });
getElement('todayButton').addEventListener('click', () => { selectedDate.setTime(today().getTime()); loadChecklist(); });
refreshCount();
loadChecklist();
loadQuestionPreview();
setInterval(refreshCount, 5000);
setInterval(updateResets, 30000);
