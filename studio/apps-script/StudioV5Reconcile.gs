/**
 * The Nomad V5 GitHub -> Master Ledger readback.
 * Install alongside StudioV5Bridge.gs in the same bound Apps Script.
 * This is an observational state mirror, NOT an acceptance authority.
 */
function studioV5StateFromIssue_(issue) {
  var names=(issue.labels||[]).map(function(l){return l.name;});
  var has=function(label){return names.indexOf(label)>=0;};
  if (issue.state==='closed') {
    return has('studio:accepted') ? 'ACCEPTED' : 'CLOSED_UNVERIFIED';
  }
  if (has('ai:needs-attention')) return 'NEEDS_ATTENTION';
  if (has('ai:blocked')) return 'BLOCKED';
  if (has('ai:review')) return 'REVIEW';
  if (has('ai:checkpointed')) return 'CHECKPOINTED';
  if (has('ai:running')) return 'RUNNING';
  if (has('ai:claimed')) return 'CLAIMED';
  if (has('ai:dispatched')) return 'DISPATCHED';
  if (has('ai:reserved')) return 'RESERVED';
  if (has('ai:ready')) return 'GITHUB_READY';
  return 'GITHUB_OTHER';
}

/** Mirrors at most 60 assigned issues per run. Never creates/issues work. */
function studioV5ReconcileLedger() {
  return studioV5WithLock_(function() {
    var sheet=studioV5Sheet_();
    var total=sheet.getLastRow();
    var reviewed=0,changes=0;
    for (var row=2; row<=total && reviewed<60; row++) {
      var task=studioV5Row_(sheet,row);
      var url=task['GitHub Issue'];
      if (!url || task.State==='MANUAL_LOCK') continue;
      var match=/^https:\/\/github[.]com\/SIGAAMDAD\/The_Nomad\/issues\/(\d+)\/?$/.exec(url);
      if (!match) {
        studioV5Field_(sheet,row,'Last Error','Invalid GitHub Issue URL');
        continue;
      }
      reviewed++;
      try {
        var issue=studioV5Github_('get','issues/'+match[1]);
        var state=studioV5StateFromIssue_(issue);
        if (task.State!==state) {
          studioV5Field_(sheet,row,'State',state);
          studioV5Field_(sheet,row,'Updated UTC',new Date().toISOString());
          changes++;
        }
        if (task['Last Error']) studioV5Field_(sheet,row,'Last Error','');
      } catch (error) {
        studioV5Field_(sheet,row,'Last Error',String(error).slice(0,280));
      }
    }
    return 'GitHub readback inspected '+reviewed+', changed '+changes+
      '; no owner approvals or merges performed';
  });
}

/** Owner-installs readback alongside the intake trigger; no model execution. */
function studioV5InstallReconcileTrigger(pin) {
  studioV5Owner_(pin);
  var triggers=ScriptApp.getProjectTriggers().filter(function(t){
    return t.getHandlerFunction()==='studioV5ReconcileLedger';
  });
  if (!triggers.length)
    ScriptApp.newTrigger('studioV5ReconcileLedger').timeBased().everyMinutes(5).create();
  return 'V5 GitHub readback trigger installed';
}
