const $ = id => document.getElementById(id);
let session = null;
try { session = JSON.parse(localStorage.getItem('accord.guest')); } catch { /* Storage can be unavailable. */ }
let token = session?.access_token ?? null;
let busy = false;
let currentPage = 'home';
let taskList = null;
function message(text) { $('message').textContent = text; }
async function api(path, method = 'GET', body) {
  const response = await fetch('/api/' + path, {
    method, signal: AbortSignal.timeout(25000), headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: 'Bearer ' + token } : {}) },
    ...(body ? { body: JSON.stringify(body) } : {})
  });
  const data = response.status === 204 ? null : await response.json();
  if (!response.ok) {
    if (response.status === 401 && session) session.expires_at = 0;
    throw new Error(data?.message || 'Không thể xử lý yêu cầu. Vui lòng thử lại.');
  }
  return data;
}
async function run(action) {
  if (busy) return;
  busy = true; message('');
  document.querySelectorAll('button').forEach(b => b.disabled = true);
  try { await action(); } catch (error) { message(error.message); }
  finally { busy = false; document.querySelectorAll('button').forEach(b => b.disabled = false); }
}
async function ensureSession() {
  if (session?.access_token && session.expires_at > Date.now() / 1000 + 60) return;
  const data = session?.refresh_token
    ? await api('session/refresh', 'POST', { refreshToken: session.refresh_token })
    : await api('session/guest', 'POST', {});
  if (!data?.access_token || !data.refresh_token) throw new Error('Không thể tạo phiên khách. Vui lòng thử lại.');
  session = { access_token: data.access_token, refresh_token: data.refresh_token, expires_at: data.expires_at ?? Date.now() / 1000 + data.expires_in };
  token = session.access_token;
  try { localStorage.setItem('accord.guest', JSON.stringify(session)); } catch { message('Trình duyệt không lưu được phiên khách. Công việc có thể không truy cập được sau khi đóng trang.'); }
}
async function loadTasks() {
  await ensureSession();
  taskList = await api('tasks');
  renderTasks();
}
function renderTasks() {
  if (taskList === null) return;
  const tasks = currentPage === 'home' ? taskList.slice(0, 5) : taskList;
  $('summary').textContent = currentPage === 'home'
    ? `Hiển thị ${tasks.length}/${taskList.length} công việc · Mới nhất trước`
    : `${taskList.filter(t => t.is_complete).length}/${taskList.length} công việc đã hoàn thành`;
  $('tasks').replaceChildren();
  if (!tasks.length) { const row = document.createElement('li'); row.textContent = 'Chưa có công việc. Vào Task để thêm công việc đầu tiên.'; $('tasks').append(row); }
  for (const task of tasks) {
    const row = document.createElement('li'); row.classList.toggle('complete', task.is_complete);
    const checkbox = document.createElement('input'); checkbox.type = 'checkbox'; checkbox.checked = task.is_complete; checkbox.setAttribute('aria-label', 'Hoàn thành: ' + task.title);
    checkbox.addEventListener('change', () => run(async () => { try { await ensureSession(); await api('tasks/' + task.id, 'PATCH', { isComplete: checkbox.checked }); await loadTasks(); } catch (error) { checkbox.checked = task.is_complete; throw error; } }));
    const title = document.createElement('span'); title.textContent = task.title;
    const remove = document.createElement('button'); remove.textContent = 'Xóa'; remove.className = 'secondary'; remove.setAttribute('aria-label', 'Xóa: ' + task.title);
    remove.addEventListener('click', () => run(async () => { await ensureSession(); await api('tasks/' + task.id, 'DELETE'); await loadTasks(); }));
    if (currentPage === 'home') {
      const status = document.createElement('small'); status.className = 'task-status';
      status.textContent = task.is_complete ? 'Hoàn thành' : 'Chưa hoàn thành';
      row.append(title, status);
    } else row.append(checkbox, title, remove);
    $('tasks').append(row);
  }
}
function navigate() {
  currentPage = location.hash === '#task' ? 'task' : 'home';
  document.title = `ACCORD · ${currentPage === 'home' ? 'Home' : 'Task'}`;
  for (const page of ['home', 'task']) {
    if (page === currentPage) $('nav-' + page).setAttribute('aria-current', 'page');
    else $('nav-' + page).removeAttribute('aria-current');
  }
  $('page-title').textContent = currentPage === 'home' ? 'Top 5 task của bạn' : 'Công việc của tôi';
  $('page-description').textContent = currentPage === 'home'
    ? '5 công việc mới nhất trong không gian của bạn.'
    : 'Thêm, hoàn thành và quản lý tất cả công việc của bạn.';
  $('task-form').hidden = currentPage !== 'task';
  $('view-all').hidden = currentPage !== 'home';
  renderTasks();
}
window.addEventListener('hashchange', navigate);
navigate();
$('reload').addEventListener('click', () => run(loadTasks));
$('task-form').addEventListener('submit', event => { event.preventDefault(); run(async () => { await ensureSession(); await api('tasks', 'POST', { title: $('title').value }); $('title').value = ''; await loadTasks(); }); });
run(loadTasks);
