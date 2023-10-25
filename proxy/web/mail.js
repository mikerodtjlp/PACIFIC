MRO.send_mail = r => {
  let b = '';
  for (let i = 0; i < 16; ++i)b += r.get('body' + i + '');
  MRO.skipunld = 1;
  window.location.href = 'mailto:?subject=' + r.get('subject') + '&body=' + encodeURIComponent(decodeURI(b));
}
