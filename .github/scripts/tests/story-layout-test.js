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
