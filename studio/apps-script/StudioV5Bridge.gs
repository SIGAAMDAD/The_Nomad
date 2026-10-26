/**
 * The Nomad V5 signed-intake companion for the existing bound Apps Script.
 *
 * Install as an ADDITIONAL source file, not as a replacement for Code.gs.
 * Nothing here activates itself or overwrites historical queue tabs.
 *
 * Script Properties:
 *   STUDIO_GITHUB_TOKEN   Fine-grained GitHub Issues read/write for The_Nomad
 *   STUDIO_APPROVAL_KEY   Same high-entropy secret as GitHub Actions secret
 *   NOMAD_OWNER_PIN_SHA256  Existing owner authorization SHA-256 PIN digest
 *   STUDIO_V5_ENABLED    Set to true only after manual verification
 */
var STUDIO_V5 = Object.freeze({
  ledger: '1avYu8wwu8KZMPcb36_9Pz0-6cJgkZCOWber4NNxx8NA',
  queue: 'STUDIO_INTAKE_V5',
  repo: 'SIGAAMDAD/The_Nomad',
  cols: ['Task ID','Title','Department','Target','Baseline SHA',
    'Drive Brief URL','Depends On','Owned Paths','Protected Paths',
    'Approval','Approval Signature','State','GitHub Issue','Last Error','Updated UTC'],
  departments: ['design','narrative','gameplay','ai','engine','rendering',
    'world','qa','integration'],
  targets: {
    game: {repo:'SIGAAMDAD/The_Nomad',branch:'Nomad3D'},
    framework:{repo:'SIGAAMDAD/NomadFramework',branch:'develop'}
  }
});

function studioV5Props_() {
  return PropertiesService.getScriptProperties();
}

function studioV5Hex_(bytes) {
  return bytes.map(function(b){ return ('0' + (b & 255).toString(16)).slice(-2); }).join('');
}

function studioV5Owner_(pin) {
  var expected = studioV5Props_().getProperty('NOMAD_OWNER_PIN_SHA256');
  if (!expected) throw new Error('Existing controller owner PIN is not configured');
  var actual = studioV5Hex_(Utilities.computeDigest(
    Utilities.DigestAlgorithm.SHA_256, String(pin), Utilities.Charset.UTF_8));
  if (actual !== expected) throw new Error('Owner PIN rejected');
}

function studioV5WithLock_(operation) {
  var lock = LockService.getScriptLock();
  if (!lock.tryLock(10000)) throw new Error('Studio V5 controller is already running');
  try { return operation(); } finally { lock.releaseLock(); }
}

function studioV5Sheet_() {
  var sheet = SpreadsheetApp.openById(STUDIO_V5.ledger).getSheetByName(STUDIO_V5.queue);
  if (!sheet) throw new Error('V5 queue missing; run studioV5InstallIntake()');
  var header = sheet.getRange(1,1,1,STUDIO_V5.cols.length).getDisplayValues()[0];
  if (JSON.stringify(header) !== JSON.stringify(STUDIO_V5.cols))
    throw new Error('V5 queue schema changed; refusing to write');
  return sheet;
}

function studioV5Row_(sheet, row) {
  var cells = sheet.getRange(row,1,1,STUDIO_V5.cols.length).getDisplayValues()[0];
  var result = {row:row};
  STUDIO_V5.cols.forEach(function(k,i){result[k]=cells[i] || '';});
  return result;
}

function studioV5Field_(sheet, row, field, value) {
  sheet.getRange(row,STUDIO_V5.cols.indexOf(field)+1).setValue(value);
}

function studioV5Contract_(task) {
  var target = STUDIO_V5.targets[task.Target];
  if (!target) throw new Error('Unsupported target');
  if (STUDIO_V5.departments.indexOf(task.Department) < 0)
    throw new Error('Unsupported department');
  if (!/^[A-Za-z0-9][A-Za-z0-9_.-]{2,79}$/.test(task['Task ID']))
    throw new Error('Invalid task ID');
  if (!task.Title || task.Title.length > 180)
    throw new Error('Invalid task title');
  if (!/^[a-fA-F0-9]{40}$/.test(task['Baseline SHA']))
    throw new Error('Pin exact source SHA');
  if (!/^https:\/\/(docs[.]google[.]com|drive[.]google[.]com)\//.test(task['Drive Brief URL']))
    throw new Error('Invalid source brief URL');
  var deps = task['Depends On'] || '-';
  var owned = task['Owned Paths'] || '-';
  var protectedPaths = task['Protected Paths'] || '-';
  if (deps !== '-' && !/^[A-Za-z0-9_,. -]+$/.test(deps))
    throw new Error('Invalid dependencies');
  if (owned !== '-' && !/^[A-Za-z0-9_/.;*+@-]+$/.test(owned))
    throw new Error('Invalid owned paths');
  if (protectedPaths !== '-' && !/^[A-Za-z0-9_/.;*+@-]+$/.test(protectedPaths))
    throw new Error('Invalid protected paths');
  if (owned === '-' && ['design','narrative','qa','integration'].indexOf(task.Department) < 0)
    throw new Error('Programming tasks need owned source paths');
  return {
    STUDIO_TASK_ID:task['Task ID'],TARGET:task.Target,
    TARGET_REPO:target.repo,TARGET_BRANCH:target.branch,
    BASE_SHA:task['Baseline SHA'],DRIVE_BRIEF_URL:task['Drive Brief URL'],
    OWNER_APPROVED:'YES',DEPARTMENT:task.Department,
    DEPENDS_ON:deps,OWNED_PATHS:owned,PROTECTED_PATHS:protectedPaths
  };
}

function studioV5Canonical_(contract) {
  var ordered = {};
  Object.keys(contract).sort().forEach(function(key){ ordered[key] = contract[key]; });
  return JSON.stringify(ordered);
}

function studioV5Signature_(contract) {
  var key = studioV5Props_().getProperty('STUDIO_APPROVAL_KEY');
  if (!key || key.length < 32) throw new Error('Missing strong STUDIO_APPROVAL_KEY');
  return studioV5Hex_(Utilities.computeHmacSha256Signature(
    studioV5Canonical_(contract), key));
}

function studioV5InstallIntake() {
  return studioV5WithLock_(function() {
    var file = SpreadsheetApp.openById(STUDIO_V5.ledger);
    var sheet = file.getSheetByName(STUDIO_V5.queue);
    if (sheet) { studioV5Sheet_(); return 'Already installed; unchanged'; }
    sheet = file.insertSheet(STUDIO_V5.queue);
    sheet.getRange(1,1,1,STUDIO_V5.cols.length).setValues([STUDIO_V5.cols]);
    sheet.getRange(1,1,1,STUDIO_V5.cols.length).setFontWeight('bold');
    sheet.setFrozenRows(1);
    return 'STUDIO_INTAKE_V5 installed in staging; no dispatch';
  });
}

/** Requires owner PIN; refuses to overwrite an approved task. */
function studioV5Approve(taskId, pin) {
  return studioV5WithLock_(function() {
    studioV5Owner_(pin);
    var sheet = studioV5Sheet_();
    var values = sheet.getDataRange().getDisplayValues();
    var matches = [];
    for (var i=1; i<values.length; i++) {
      if (values[i][0] === taskId) matches.push(i+1);
    }
    if (matches.length !== 1) throw new Error('Require exactly one task ID');
    var row = studioV5Row_(sheet,matches[0]);
    if (row.State !== 'DRAFT' || row.Approval || row['Approval Signature'])
      throw new Error('Task must be unapproved DRAFT');
    var contract = studioV5Contract_(row);
    var sig = studioV5Signature_(contract);
    studioV5Field_(sheet,row.row,'Approval','OWNER_APPROVED');
    studioV5Field_(sheet,row.row,'Approval Signature',sig);
    studioV5Field_(sheet,row.row,'State','READY');
    studioV5Field_(sheet,row.row,'Updated UTC',new Date().toISOString());
    return 'Approved ' + taskId;
  });
}

function studioV5Mode_() {
  if (studioV5Props_().getProperty('STUDIO_V5_ENABLED') !== 'true')
    return false;
  var file = SpreadsheetApp.openById(STUDIO_V5.ledger);
  var rows = file.getSheetByName('CONFIG').getDataRange().getDisplayValues();
  for (var i=0; i<rows.length; i++) {
    if (rows[i][0] === 'studio_mode') return rows[i][1] === 'ACTIVE';
  }
  return false;
}

function studioV5Github_(method, path, body) {
  var token = studioV5Props_().getProperty('STUDIO_GITHUB_TOKEN');
  if (!token) throw new Error('Missing STUDIO_GITHUB_TOKEN');
  var options = {method:method,muteHttpExceptions:true,
    headers:{Authorization:'Bearer ' + token,
      Accept:'application/vnd.github+json',
      'X-GitHub-Api-Version':'2022-11-28'}};
  if (body) { options.contentType='application/json'; options.payload=JSON.stringify(body); }
  var resp=UrlFetchApp.fetch('https://api.github.com/repos/' + STUDIO_V5.repo + '/' + path,options);
  if (resp.getResponseCode() < 200 || resp.getResponseCode() >= 300)
    throw new Error('GitHub HTTP ' + resp.getResponseCode());
  return JSON.parse(resp.getContentText());
}

function studioV5Existing_(taskId) {
  for (var p=1; p<=10; p++) {
    var results=studioV5Github_('get','issues?state=all&per_page=100&page='+p);
    for (var j=0; j<results.length; j++) {
      if (!results[j].pull_request &&
          (results[j].body||'').split('\n').indexOf('STUDIO_TASK_ID: '+taskId)>=0)
        return results[j];
    }
    if (results.length<100) return null;
  }
  throw new Error('Issue search limit exceeded; refusing duplicate');
}

/** A five-minute trigger may call this. Each task must be owner-signed first. */
function studioV5DispatchIntake() {
  return studioV5WithLock_(function() {
    if (!studioV5Mode_()) return 'LOCKED: no issue writes';
    var sheet=studioV5Sheet_();
    var total=sheet.getLastRow(), sent=0;
    for (var row=2; row<=total && sent<10; row++) {
      var task=studioV5Row_(sheet,row);
      if (task.State!=='READY' || task.Approval!=='OWNER_APPROVED' ||
          task['GitHub Issue']) continue;
      try {
        var contract=studioV5Contract_(task);
        if (studioV5Signature_(contract) !== task['Approval Signature'])
          throw new Error('Signed assignment was modified');
        var lines=Object.keys(contract).sort().map(function(k){
          return k + ': ' + contract[k];
        });
        lines.push('APPROVAL_SIG: ' + task['Approval Signature']);
        var found=studioV5Existing_(task['Task ID']);
        var issue=found || studioV5Github_('post','issues',{
          title:'[' + task['Task ID'] + '] ' + task.Title,
          body:lines.join('\n') + '\n\nAcceptance criteria: see approved Drive brief.',
          labels:['studio:approved','ai:ready','dept:' + task.Department]
        });
        studioV5Field_(sheet,row,'GitHub Issue',issue.html_url);
        studioV5Field_(sheet,row,'State','GITHUB_READY');
        studioV5Field_(sheet,row,'Last Error','');
        studioV5Field_(sheet,row,'Updated UTC',new Date().toISOString());
        sent++;
      } catch (error) {
        studioV5Field_(sheet,row,'Last Error',String(error).slice(0,300));
      }
    }
    return 'Reconciled '+sent+' approved issues';
  });
}

function studioV5InstallTrigger(pin) {
  studioV5Owner_(pin);
  var triggers=ScriptApp.getProjectTriggers().filter(function(t){
    return t.getHandlerFunction()==='studioV5DispatchIntake';
  });
  if (!triggers.length)
    ScriptApp.newTrigger('studioV5DispatchIntake').timeBased().everyMinutes(5).create();
  return 'V5 5-minute trigger installed; kill switches remain authoritative';
}
