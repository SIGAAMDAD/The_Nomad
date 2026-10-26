/**
 * The Nomad Studio V5 — spreadsheet owner controls.
 * Install as a separate bound Apps Script source file alongside V5 Bridge
 * and Reconcile. No source hard-codes the owner's PIN or credentials.
 */
function studioV5ShowMenu() {
  SpreadsheetApp.getUi().createMenu('Nomad Studio V5')
    .addItem('Initialize V5 intake tab', 'studioV5MenuInitialize')
    .addItem('Approve selected DRAFT task', 'studioV5MenuApproveSelected')
    .addItem('Install V5 5-minute triggers', 'studioV5MenuInstallTriggers')
    .addItem('Pause V5 Drive intake', 'studioV5MenuPauseIntake')
    .addToUi();
}

function studioV5MenuOwnerPin_() {
  var ui = SpreadsheetApp.getUi();
  var response = ui.prompt('The Nomad Studio — Owner authorization',
    'Enter the current Studio owner PIN (never enter it into a Sheet cell):',
    ui.ButtonSet.OK_CANCEL);
  if (response.getSelectedButton() !== ui.Button.OK) return null;
  var pin = response.getResponseText();
  studioV5Owner_(pin);
  return pin;
}

function studioV5MenuInitialize() {
  SpreadsheetApp.getUi().alert(studioV5InstallIntake());
}

function studioV5MenuApproveSelected() {
  var ui=SpreadsheetApp.getUi();
  var active=SpreadsheetApp.getActiveSpreadsheet();
  if (active.getId()!==STUDIO_V5.ledger)
    throw new Error('Wrong spreadsheet');
  if (active.getActiveSheet().getName()!==STUDIO_V5.queue)
    throw new Error('Select a DRAFT task row on STUDIO_INTAKE_V5');
  var range=active.getActiveRange();
  if (!range || range.getRow() < 2)
    throw new Error('Select a DRAFT task row, not the header');
  var task=studioV5Row_(studioV5Sheet_(),range.getRow());
  if (!task['Task ID']) throw new Error('Selected row has no Task ID');
  var pin=studioV5MenuOwnerPin_();
  if (pin===null) return;
  ui.alert(studioV5Approve(task['Task ID'],pin));
}

function studioV5MenuInstallTriggers() {
  var pin=studioV5MenuOwnerPin_();
  if (pin===null) return;
  studioV5InstallTrigger(pin);
  studioV5InstallReconcileTrigger(pin);
  SpreadsheetApp.getUi().alert(
    'Triggers installed. New dispatch still requires CONFIG.studio_mode=ACTIVE '+
    'and Script Property STUDIO_V5_ENABLED=true.');
}

function studioV5MenuPauseIntake() {
  var pin=studioV5MenuOwnerPin_();
  if (pin===null) return;
  studioV5Props_().setProperty('STUDIO_V5_ENABLED','false');
  SpreadsheetApp.getUi().alert(
    'New Drive-to-GitHub intake paused. This does not stop existing '+
    'ChatGPT Work sessions or change the GitHub kill switch.');
}

/** Run once from editor; menu appears on subsequent spreadsheet opens. */
function studioV5InstallMenuTrigger() {
  var available=ScriptApp.getProjectTriggers().some(function(trigger){
    return trigger.getHandlerFunction()==='studioV5ShowMenu';
  });
  if (!available) {
    ScriptApp.newTrigger('studioV5ShowMenu')
      .forSpreadsheet(STUDIO_V5.ledger).onOpen().create();
  }
  studioV5ShowMenu();
  return 'V5 owner menu installed';
}
