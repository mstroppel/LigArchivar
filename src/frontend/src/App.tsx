import React, { useState } from 'react';
import './App.css';

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    // Simple authentication simulation
    if (username === 'admin' && password === 'changeme') {
      setIsLoggedIn(true);
    }
  };

  if (!isLoggedIn) {
    return (
      <div className="login-container">
        <h1>FL.LigArchivar Login</h1>
        <form onSubmit={handleLogin} className="login-form">
          <div>
            <label>Username:</label>
            <input 
              type="text" 
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required 
            />
          </div>
          <div>
            <label>Password:</label>
            <input 
              type="password" 
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required 
            />
          </div>
          <button type="submit">Login</button>
        </form>
      </div>
    );
  }

  return (
    <div className="app-container">
      <h1>FL.LigArchivar Archive Manager</h1>
      <p>Archive management application with tree view and file operation capabilities</p>
      <div className="tree-view">
        <h2>Archive Structure</h2>
        <div className="tree-item">Digitalfoto</div>
        <div className="tree-item">Ton</div>
        <div className="tree-item">Video</div>
      </div>
      <div className="event-details">
        <h2>Event Details</h2>
        <p>Select an event to view and manage files</p>
      </div>
    </div>
  );
}

export default App;