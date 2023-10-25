var TXT = {};
TXT.btn_top = lid => {
  editor.resize(true);
  editor.scrollToLine(0, true, true, function () { });
  editor.gotoLine(0, 0, true);
}
TXT.btn_bottom = lid => {
  const row = editor.session.getLength() - 1;
  const column = editor.session.getLine(row).length; // or simply Infinity
  editor.gotoLine(row + 1, column, true);
}
TXT.btn_page_up = lid => { editor.scrollPageUp(); }
TXT.btn_page_down = lid => { editor.scrollPageDown(); }
TXT.btn_up = lid => { editor.navigateUp(1); }
TXT.btn_down = lid => { editor.navigateDown(1); }