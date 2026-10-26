/**
 * The Nomad Studio V4 intake bridge.
 * ADD AS A SEPARATE FILE to the existing spreadsheet-bound Apps Script project.
 * Does not override Code.gs. Does not activate itself.
 *
 * Script Properties:
 *   STUDIO_GITHUB_TOKEN: fine-grained token, Issues: Read/Write, repo The_Nomad
 *   NOMAD_OWNER_PIN_SHA256: existing controller owner PIN hash
 *
 * Spreadsheet STUDIO_INTAKE fields are created by studioV4InstallIntake().
 * A row must be approved through studioV4ApproveIntake(), not direct cell edits.
 */
var STUDIO_V4 = Object.freeze({
  ledgerId: '1avYu8wwu8KZMPcb36_9Pz0-6cJgkZCOWber4NNxx8NA',
  repo: 'SIGAAMDAD/The_Nomad',
  sheet: 'STUDIO_INTAKE',
  columns: ['Task ID', 'Title', 'Department', 'Target', 'Baseline SHA', 'Drive Brief URL',
    'Approval', 'State', 'GitHub Issue', 'Last Error', 'Updated UTC'],
  targets: {
    game: {repo: 'SIGAAMDAD/The_Nomad', branch: 'Nomad3D'},
    framework: {repo: 'SIGAAMDAD/NomadFramework', branch: 'develop'}
  },
  departments: ['design','narrative','gameplay','ai','engine','rendering','world','qa','integration']
});

function studioV4Sheet_() {
  var sheet = SpreadsheetApp.openById(STUDIO_V4.ledgerId).getSheetByName(STUDIO_V4.sheet);
  if (!sheet) throw new Error('Missing STUDIO_INTAKE; run studioV4InstallIntake()');
  return sheet;
}

function studioV4WithLock_(callback) {
  var lock = LockService.getScriptLock();
  if (!lock.tryLock(10000)) throw new Error('Another intake operation is running');
  try { return callback(); } finally { lock.releaseLock(); }
}

function studioV4Owner_(pin) {
  var expected = PropertiesService.getScriptProperties().getProperty('NOMAD_OWNER_PIN_SHA256');
  if (!expected) throw new Error('Configure owner through existing controller');
  var hash = Utilities.computeDigest(Utilities.DigestAlgorithm.SHA_256,
    String(pin), Utilities.Charset.UTF_8)
      .map(function(b) { return ('0' + (b & 255).toString(16)).slice(-2); }).join('');
  if (expected !== hash) throw new Error('Owner authorization required');
}

function studioV4ReadRows_() {
  var sheet = studioV4Sheet_();
  var data = sheet.getDataRange().getDisplayValues();
  var headers = data[0];
  STUDIO_V4.columns.forEach(function(c) {
    if (headers.indexOf(c) < 0) throw new Error('Missing column: ' + c);
  });
  return data.slice(1).map(function(values, index) {
    var record = {row: index + 2};
    headers.forEach(function(h, i) { record[h] = values[i] || ''; });
    return record;
  }).filter(function(r) { return !!r['Task ID']; });
}

function studioV4Write_(sheet, row, field, value) {
  sheet.getRange(row, STUDIO_V4.columns.indexOf(field) + 1).setValue(value);
}

function studioV4Validate_(row) {
  var id = row['Task ID'];
  if (!/^[A-Za-z0-9][A-Za-z0-9_.-]{2,79}$/.test(id)) throw new Error('Bad Task ID');
  if (!row.Title || row.Title.length > 180) throw new Error('Invalid Title');
  if (STUDIO_V4.departments.indexOf(row.Department) < 0) throw new Error('Invalid Department');
  var target = STUDIO_V4.targets[row.Target];
  if (!target) throw new Error('Invalid Target');
  if (!/^[a-f0-9]{40}$/i.test(row['Baseline SHA'])) throw new Error('Pin exact 40-digit baseline');
  if (!/^https:\/\/(docs\.google\.com|drive\.google\.com)\//.test(row['Drive Brief URL']))
    throw new Error('Use verified Drive document URL');
  return target;
}

/** Owner runs once in the Apps Script editor. Never deletes another tab. */
function studioV4InstallIntake() {
  return studioV4WithLock_(function() {
    var file = SpreadsheetApp.openById(STUDIO_V4.ledgerId);
    var existing = file.getSheetByName(STUDIO_V4.sheet);
    if (existing) return 'STUDIO_INTAKE already exists; unchanged';
    var sheet = file.insertSheet(STUDIO_V4.sheet);
    sheet.getRange(1, 1, 1, STUDIO_V4.columns.length).setValues([STUDIO_V4.columns]);
    sheet.setFrozenRows(1);
    sheet.getRange(1, 1, 1, STUDIO_V4.columns.length).setFontWeight('bold');
    return 'Created STUDIO_INTAKE; no work dispatched';
  });
}

/** Owner-only approval: editing the Approval cell manually is not enough. */
function studioV4ApproveIntake(taskId, pin) {
  return studioV4WithLock_(function() {
    studioV4Owner_(pin);
    var sheet = studioV4Sheet_();
    var rows = studioV4ReadRows_().filter(function(r) { return r['Task ID'] === taskId; });
    if (rows.length !== 1) throw new Error('Require exactly one matching task');
    var task = rows[0];
    if (task.State !== 'DRAFT' || task.Approval) throw new Error('Only unapproved DRAFT');
    studioV4Validate_(task);
    studioV4Write_(sheet, task.row, 'Approval', 'OWNER_APPROVED');
    studioV4Write_(sheet, task.row, 'State', 'READY');
    studioV4Write_(sheet, task.row, 'Updated UTC', new Date().toISOString());
    return 'Approved intake ' + taskId;
  });
}

/** Read one GitHub response. Call only with a pinned, correctly scoped credential. */
function studioV4GitHub_(method, path, payload) {
  var token = PropertiesService.getScriptProperties().getProperty('STUDIO_GITHUB_TOKEN');
  if (!token) throw new Error('Set STUDIO_GITHUB_TOKEN in Script Properties');
  var options = {
    method: method, muteHttpExceptions: true,
    headers: {
      Authorization: 'Bearer ' + token,
      Accept: 'application/vnd.github+json',
      'X-GitHub-Api-Version': '2022-11-28'
    }
  };
  if (payload) { options.contentType = 'application/json'; options.payload = JSON.stringify(payload); }
  var response = UrlFetchApp.fetch('https://api.github.com/repos/' +
    STUDIO_V4.repo + '/' + path, options);
  var status = response.getResponseCode();
  if (status < 200 || status >= 300) throw new Error('GitHub HTTP ' + status +
    ': ' + response.getContentText().slice(0, 300));
  return JSON.parse(response.getContentText());
}

/** Avoid duplicates after intermittent errors by checking the latest issues first. */
function studioV4ExistingIssue_(taskId) {
  for (var page = 1; page <= 5; page++) {
    var issues = studioV4GitHub_('get', 'issues?state=all&per_page=100&page=' + page);
    for (var i = 0; i < issues.length; i++) {
      var marker = 'STUDIO_TASK_ID: ' + taskId;
      if (!issues[i].pull_request && (issues[i].body || '').split('\n').indexOf(marker) >= 0)
        return issues[i];
    }
    if (issues.length < 100) break;
  }
  return null;
}

function studioV4DispatchIntake() {
  return studioV4WithLock_(function() {
    // Existing studio lock is always authoritative.
    var file = SpreadsheetApp.openById(STUDIO_V4.ledgerId);
    var config = file.getSheetByName('CONFIG').getDataRange().getDisplayValues();
    var cfg = {};
    config.slice(1).forEach(function(r) { cfg[r[0]] = r[1]; });
    if (cfg.studio_mode !== 'ACTIVE') return 'LOCKED: No intake dispatched';
    var sheet = studioV4Sheet_();
    var ready = studioV4ReadRows_().filter(function(r) {
      return r.State === 'READY' && r.Approval === 'OWNER_APPROVED' && !r['GitHub Issue'];
    }).slice(0, 10);
    var done = 0;
    for (var i = 0; i < ready.length; i++) {
      var task = ready[i];
      try {
        var target = studioV4Validate_(task);
        var body = 'STUDIO_TASK_ID: ' + task['Task ID'] +
          '\nTARGET: ' + task.Target +
          '\nTARGET_REPO: ' + target.repo +
          '\nTARGET_BRANCH: ' + target.branch +
          '\nBASE_SHA: ' + task['Baseline SHA'] +
          '\nDRIVE_BRIEF_URL: ' + task['Drive Brief URL'] +
          '\nOWNER_APPROVED: YES\n\nAcceptance criteria and owned paths: see approved Drive brief.';
        var existing = studioV4ExistingIssue_(task['Task ID']);
        var issue = existing || studioV4GitHub_('post', 'issues', {
          title: '[' + task['Task ID'] + '] ' + task.Title,
          body: body,
          labels: ['dept:' + task.Department, 'studio:approved', 'ai:ready']
        });
        studioV4Write_(sheet, task.row, 'GitHub Issue', issue.html_url);
        studioV4Write_(sheet, task.row, 'State', 'GITHUB_READY');
        studioV4Write_(sheet, task.row, 'Last Error', '');
        studioV4Write_(sheet, task.row, 'Updated UTC', new Date().toISOString());
        done++;
      } catch (e) {
        studioV4Write_(sheet, task.row, 'Last Error', String(e).slice(0, 400));
      }
    }
    return 'Created or reconciled ' + done + ' GitHub issues';
  });
}

/** Installer intentionally does not call studioV4DispatchIntake. */
function studioV4InstallTrigger(pin) {
  studioV4Owner_(pin);
  var triggers = ScriptApp.getProjectTriggers().filter(function(t) {
    return t.getHandlerFunction() === 'studioV4DispatchIntake';
  });
  if (!triggers.length) {
    ScriptApp.newTrigger('studioV4DispatchIntake').timeBased().everyMinutes(5).create();
  }
  return 'Trigger installed; respects CONFIG studio_mode';
}
