import { useState, useEffect } from 'react'
import './App.css'

function App() {
  const [command, setCommand] = useState('');
  const [log, setLog] = useState([]);
  const [theme, setTheme] = useState('dark');

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', theme);
  }, [theme]);

  const looksLikeCommand = (text) => {
    if (!text) return true;
    const t = text.trim().toUpperCase();
    if (t.startsWith('PLACE')) return /^PLACE\s+-?\d+\s*,\s*-?\d+(?:\s*,\s*[A-Z]+)?$/i.test(text);
    return ['MOVE','LEFT','RIGHT','REPORT'].includes(t);
  };

  const postCommand = async () => {
    if (!command.trim()) return;
    const time = new Date().toLocaleTimeString();
    setLog(l => [{ id: Date.now(), txt: `${time} - ${command} - …`, status: 'pending' }, ...l]);

    try {
      const res = await fetch('/robot/command', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ command })
      });

      const contentType = res.headers.get('content-type') ?? '';
      let responseBody = '';
      let json = null;

      // First, read the response body
      try {
        if (contentType.includes('application/json')) {
          json = await res.json();
          responseBody = JSON.stringify(json);
        } else {
          responseBody = await res.text();
        }
      } catch (parseErr) {
        responseBody = `[Parse error: ${parseErr.message}]`;
      }

      // Format the result
      let entry;
      if (!res.ok) {
        // HTTP error (4xx, 5xx)
        const fullError = `HTTP ${res.status} ${res.statusText} - ${responseBody}`;
        entry = { id: Date.now(), txt: `${time} - ${command} - ERROR - ${fullError}`, status: 'error' };
      } else if (json) {
        // Success with JSON response
        const formatted = formatEntry(command, json);
        entry = { id: Date.now(), txt: formatted.text, status: formatted.status };
      } else {
        // Success without JSON
        entry = { id: Date.now(), txt: `${time} - ${command} - OK - ${responseBody}`, status: 'success' };
      }

      setLog(l => [entry, ...l.filter(x => x.status !== 'pending')]);
    } catch (e) {
      const entry = { id: Date.now(), txt: `${time} - ${command} - ERROR - ${String(e)}`, status: 'error' };
      setLog(l => [entry, ...l.filter(x => x.status !== 'pending')]);
    }

    setCommand('');
  };

  const formatEntry = (cmd, result) => {
    const time = new Date().toLocaleTimeString();
    if (!result) return { text: `${time} - ${cmd} - No response`, status: 'error' };
    if (result.success) {
      if (result.report) return { text: `${time} - ${cmd} - REPORT: ${result.report}`, status: 'report' };
      return { text: `${time} - ${cmd} - OK - ${result.message}`, status: 'success' };
    } else {
      return { text: `${time} - ${cmd} - ERROR - ${result.message}`, status: 'error' };
    }
  };

  const toggleTheme = () => setTheme(t => (t === 'dark' ? 'light' : 'dark'));
  const autofill = () => setCommand('PLACE 0,0,NORTH');

  return (
    <div className="app-root">
      <header className="app-header">
        <div className="header-content">
          <div className="title-row">
            <h1>Toy Robot</h1>
            <button className="theme-toggle" onClick={toggleTheme} aria-label="Toggle theme">
              {theme === 'dark' ? '🌞' : '🌙'}
            </button>
          </div>
          <p className="subtitle">Simulator for a toy robot on a 6x6 grid</p>
        </div>
      </header>

      <section className="main-container">
        {/* Left: Robot panel */}
        <aside className="robot-panel">
          <img className="robot-image" src="/robot.png" alt="Toy Robot" />
        </aside>    

        {/* Center: Input and Output panels */}
        <div className="input-output-wrapper">
          {/* Input Panel */}
          <div className="input-panel">
            <div className="input-header">
              <h3>Input Command</h3>
            </div>
            <div className="command-area">
              <input
                className={`command-input ${looksLikeCommand(command) ? '' : 'input-invalid'}`}
                placeholder="PLACE 0,0,NORTH  —  MOVE | LEFT | RIGHT | REPORT"
                value={command}
                onChange={(e) => setCommand(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter') postCommand(); }}
                aria-label="Command"
              />
              <button className="go-button" onClick={postCommand}>Go</button>
            </div>
          </div>

          {/* Output Panel */}
          <div className="output-panel">
            <div className="output-header">
              <h3>Output Log</h3>
            </div>
            <div className="log-box" role="log" aria-live="polite">
              {log.length === 0 && <div className="log-empty">Ready for commands...</div>}
              {log.map(entry => (
                <div key={entry.id} className={`log-entry log-${entry.status}`}>
                  {entry.txt}
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Right: Command Reference */}
        <aside className="reference-panel">
          <button className="fill-button" onClick={autofill}>Fill example</button>
          
          <h4>Command Reference</h4>
          <ul className="reference-list">
            <li><code>PLACE X,Y,DIRECTION</code></li>
            <li><code>MOVE</code></li>
            <li><code>LEFT</code></li>
            <li><code>RIGHT</code></li>
            <li><code>REPORT</code></li>
          </ul>

          <div className="example-box">
            <h5>Example</h5>
            <pre className="example-io">PLACE 0,0,NORTH
MOVE
REPORT</pre>
            <div className="example-output">Output: 0,1,NORTH</div>
          </div>
        </aside>
      </section>
    </div>
  )
}

export default App
