const $ = id => document.getElementById(id);
let toastTimer;

async function request(url, options) {
  const response = await fetch(url, options);
  if (!response.ok) throw new Error(`Request failed (${response.status})`);
  const body = await response.text();
  return body ? JSON.parse(body) : null;
}

function notify(message, error = false) {
  const toast = $('toast');
  toast.textContent = message;
  toast.className = `toast show${error ? ' error' : ''}`;
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => toast.className = 'toast', 3500);
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

async function loadQuestions() {
  try {
    const questions = await request('/bookmarks/api/questions');
    const open = questions.filter(q => !q.isAnswered);
    const answered = questions.filter(q => q.isAnswered);
    $('openCount').textContent = `${open.length} open`;
    $('answeredCount').textContent = `${answered.length} answered`;
    $('openQuestions').replaceChildren(...(open.length ? open.map(renderQuestion) : [emptyState('No open questions', 'Add something you are curious about.') ]));
    $('answeredQuestions').replaceChildren(...(answered.length ? answered.map(renderQuestion) : [emptyState('Nothing answered yet', 'Your discoveries will appear here.') ]));
  } catch {
    $('openQuestions').replaceChildren(emptyState('Could not load questions', 'Try refreshing the page.'));
    $('answeredQuestions').replaceChildren();
    notify('Could not load questions.', true);
  }
}

function renderQuestion(question) {
  const card = document.createElement('article');
  card.className = `question-card${question.isAnswered ? ' answered' : ''}`;
  const body = document.createElement('div');
  body.className = 'question-body';
  const title = document.createElement('h3');
  title.textContent = question.text;
  body.append(title);
  if (question.answer) {
    const answer = document.createElement('p');
    answer.className = 'question-answer';
    answer.textContent = question.answer;
    body.append(answer);
  }
  const actions = document.createElement('div');
  actions.className = 'question-actions';
  const toggle = document.createElement('button');
  toggle.type = 'button';
  toggle.textContent = question.isAnswered ? 'Mark unanswered' : 'Mark answered';
  toggle.addEventListener('click', async () => {
    toggle.disabled = true;
    try {
      await request(`/bookmarks/api/questions/${question.id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ text: question.text, answer: question.answer, isAnswered: !question.isAnswered }) });
      await loadQuestions();
    } catch { toggle.disabled = false; notify('Could not update this question.', true); }
  });
  const edit = document.createElement('button');
  edit.type = 'button';
  edit.textContent = 'Edit / add answer';
  edit.addEventListener('click', () => editQuestion(question, card));
  const remove = document.createElement('button');
  remove.type = 'button';
  remove.className = 'danger-button';
  remove.textContent = 'Delete';
  remove.addEventListener('click', async () => {
    if (!confirm(`Delete “${question.text}”?`)) return;
    try { await request(`/bookmarks/api/questions/${question.id}`, { method: 'DELETE' }); await loadQuestions(); }
    catch { notify('Could not delete this question.', true); }
  });
  actions.append(toggle, edit, remove);
  card.append(body, actions);
  return card;
}

function editQuestion(question, card) {
  const form = document.createElement('form');
  form.className = 'question-edit';
  form.innerHTML = '<label>Question<input name="text" type="text" maxlength="500" required></label><label>Answer (optional)<textarea name="answer" maxlength="4000" rows="4" placeholder="What did you find out?"></textarea></label><label class="one-time-option"><input name="answered" type="checkbox"> Answered</label><p class="form-error" hidden></p><div class="form-actions"><button type="button" class="cancel">Cancel</button><button type="submit">Save question</button></div>';
  form.elements.text.value = question.text;
  form.elements.answer.value = question.answer || '';
  form.elements.answered.checked = question.isAnswered;
  form.querySelector('.cancel').addEventListener('click', () => { form.replaceWith(card); });
  form.addEventListener('submit', async event => {
    event.preventDefault();
    const text = form.elements.text.value.trim();
    if (!text) return;
    const submit = form.querySelector('[type=submit]');
    submit.disabled = true;
    try {
      await request(`/bookmarks/api/questions/${question.id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ text, answer: form.elements.answer.value.trim() || null, isAnswered: form.elements.answered.checked }) });
      await loadQuestions();
      notify('Question saved.');
    } catch {
      form.querySelector('.form-error').textContent = 'Could not save this question.';
      form.querySelector('.form-error').hidden = false;
      submit.disabled = false;
    }
  });
  card.replaceWith(form);
  form.elements.text.focus();
}

$('newQuestionForm').addEventListener('submit', async event => {
  event.preventDefault();
  const form = event.currentTarget;
  const text = form.elements.text.value.trim();
  if (!text) return;
  const submit = form.querySelector('[type=submit]');
  submit.disabled = true;
  try {
    await request('/bookmarks/api/questions', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ text, answer: null, isAnswered: false }) });
    form.reset();
    $('newQuestionError').hidden = true;
    await loadQuestions();
    notify('Question added.');
  } catch {
    $('newQuestionError').textContent = 'Could not add this question.';
    $('newQuestionError').hidden = false;
  } finally { submit.disabled = false; }
});

loadQuestions();
