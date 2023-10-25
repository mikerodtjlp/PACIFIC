var CTR = {};
// Phone stuff
CTR.phoneStr = '';
CTR.formattedStr = '';
CTR.deleteMode = false;
CTR.defaultFormat = '({0}{1}{2}) {3}{4}{5}-{6}{7}{8}{9}';
CTR.phone_keydown = (e) => { CTR.deleteMode = e.key === 'Backspace'; }
CTR.phone_input = (e) => {
  let phoneInput = e.srcElement;
  if (CTR.deleteMode) {
    phoneInput.value = phoneInput.value;
    CTR.phoneStr = CTR.parsePhoneString(phoneInput.value);
  }
  else {
    if (e.inputType == 'insertText' && !isNaN(parseInt(e.data))) {
      if (CTR.phoneStr.length <= 10) CTR.phoneStr += e.data;
    }
    phoneInput.value = CTR.formatPhoneString();
  }
}
CTR.formatPhoneString = () => {
  let strArr = CTR.phoneStr.split('');
  CTR.formattedStr = CTR.defaultFormat;
  for (let i = 0; i < strArr.length; i++) {
    CTR.formattedStr = CTR.formattedStr.replace(`{${i}}`, strArr[i]);
  }
  if (CTR.formattedStr.indexOf('{') === -1)
    return CTR.formattedStr; else
    return CTR.formattedStr.substring(0, CTR.formattedStr.indexOf('{'));
}
CTR.parsePhoneString = str => str.replace(' ', '').replace('(', '').replace(')', '').replace('-', '');

// general stuff
CTR.calc = el => { el.value = eval('(' + el.value + ').toFixed(2);'); }
// Restricts input for the given textbox to the given inputFilter.
CTR.InpFilter = (el, filter) => {
  ["input", "keydown", "keyup", "mousedown", "mouseup", "select", "contextmenu", "drop"].forEach(function (event) {
    el.addEventListener(event, function () {
      if (filter(this.value)) {
        this.oldValue = this.value;
        this.oldSelectionStart = this.selectionStart;
        this.oldSelectionEnd = this.selectionEnd;
      } else if (this.hasOwnProperty("oldValue")) {
        this.value = this.oldValue;
        this.setSelectionRange(this.oldSelectionStart, this.oldSelectionEnd);
      } else {
        this.value = "";
      }
    });
  });
}
