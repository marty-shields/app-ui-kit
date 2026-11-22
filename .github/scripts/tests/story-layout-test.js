// Serves a stub iframe.html for check-story-layout.mjs: some Card stories throw or log an error, the rest are clean.
const { createServer } = require('node:http');

const scripts = {
  'components-layout-card--title': "throw new Error('boom from the page');",
  'components-layout-card--compact': "console.error('Unhandled exception rendering component: bad attribute\\n   at Card');",
  // Allowed for its own story (AppErrorBoundary / Error) but not for another story.
  'components-layout-card--dark': "console.error('fail: Shields.UI.Components.Layout.AppErrorBoundary[9101]\\n      logged on purpose');",
  'components-layout-apperrorboundary--error': "console.error('fail: Shields.UI.Components.Layout.AppErrorBoundary[9101]\\n      logged on purpose');",
  // The parser accepts `class="x""` and makes an attribute named `"`.
  'components-layout-card--light': "document.getElementById('app').innerHTML = '<div class=\"x\"\" >styled</div>';",
  // Starts logging once the checker has looked at the page (its placeholder probe is the signal) and keeps on until the tab moves on, so the story that tab checks next must not be blamed for it.
  'components-layout-card--header-only': "document.getElementById('app').innerHTML = '<input placeholder=\"x\">'; new MutationObserver(() => setInterval(() => console.error('left over from Header only'), 1)).observe(document.body, { childList: true });",
  // A one-line clamp hides the wrapped lines, so it passes; the same text unclamped wraps and fails.
  'components-layout-card--no-title': "document.getElementById('app').innerHTML = '<span style=\"display:-webkit-box;-webkit-box-orient:vertical;-webkit-line-clamp:1;overflow-wrap:anywhere;overflow:hidden;width:50px\">Breakfastlonglonglong</span>';",
  'components-layout-card--heading-level-3': "document.getElementById('app').innerHTML = '<span style=\"display:block;overflow-wrap:anywhere;width:50px\">Breakfastlonglonglong</span>';",
};

const server = createServer((request, response) => {
  const url = new URL(request.url, 'http://localhost');
  const id = url.searchParams.get('id');
  const page = `<!DOCTYPE html><html><body><div id="app"><div>story</div></div>
<script>${scripts[id] ?? ''}</script>
<script>document.documentElement.classList.add('_blazing_story_ready_for_visible');</script></body></html>`;
  response.writeHead(200, { 'content-type': 'text/html' }).end(page);
});
server.listen(0, '127.0.0.1', () => console.log(server.address().port));
